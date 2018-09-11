using System;
using System.Linq;
using System.Threading.Tasks;
using ChessDotNet;
using FluentAssertions;
using Moq;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.Common.Events;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;
using OrleansChess.Grains.UnitTests.Extensions;
using Xunit;

namespace OrleansChess.Grains.UnitTests.GameTests {
    public class ETagTestStreamingTests : TestKitBase {

        private const string fen = "fen";
        private const string orig = "orig";
        private const string @new = "new";
        private const string eTag = "eTag";

        private TestKitSilo BuildSut (Guid gameId) {
            var mockStreamProvider = new Mock<IPlayerMoveStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);
            var game = Silo.AddProbe<IGame> (gameId);
            var boardState = new Common.BoardState (fen, orig, @new, eTag);
            game.Setup (x => x.GetBoardState ()).ReturnsAsync (boardState);
            game.Setup (x => x.ApplyValidatedMove (It.IsAny<Move> ())).ReturnsAsync (boardState);
            game.Setup (x => x.IsValidMove (It.IsAny<Move> ())).ReturnsAsync (true);
            return Silo;
        }

        [Fact]
        public async Task WhiteMove_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut (gameId);
            var stream = sut.AddStreamProbe<PlayerIMoved> (gameId, nameof (PlayerIMoved));

            var board = sut.CreateGrain<Board> (id: gameId);
            var result = await board.PlayerIMove ("A2", "A4", gameId.ToString ());

            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => {
                x.ETag.Should().Be(eTag);
                x.Fen.Should().Be(fen);
                x.NewPosition.Should().Be(@new);
                x.OriginalPosition.Should().Be(orig);
                return true;
            });
        }

        [Fact]
        public async Task BlackMove_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut (gameId);
            var stream = sut.AddStreamProbe<PlayerIIMoved> (gameId, nameof (PlayerIIMoved));

            var board = sut.CreateGrain<Board> (id: gameId);
            var eTagFromWhiteMove = await board.PlayerIMove ("A2", "A4", gameId.ToString ()).ContinueWith(x => x.Result.Data.ETag);
            var result = await board.PlayerIIMove ("A7", "A5", eTagFromWhiteMove);

            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => {
                x.ETag.Should().Be(eTag);
                x.Fen.Should().Be(fen);
                x.NewPosition.Should().Be(@new);
                x.OriginalPosition.Should().Be(orig);
                return true;
            });
        }

        [Fact]
        public async Task WhiteThenBlackThenWhiteThenBlack_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut (gameId);
            var whiteStream = sut.AddStreamProbe<PlayerIMoved> (gameId, nameof (PlayerIMoved));
            var blackStream = sut.AddStreamProbe<PlayerIIMoved> (gameId, nameof (PlayerIIMoved));

            var board = sut.CreateGrain<Board> (id: gameId);
            await board.PlayerIMove ("A2", "A4", gameId.ToString ());
            await board.PlayerIIMove ("A7", "A5", eTag);
            await board.PlayerIMove ("B2", "B4", eTag);
            await board.PlayerIIMove ("B7", "B5", eTag);

            blackStream.Sends.Should ().Be (2);
            whiteStream.Sends.Should ().Be (2);
        }
    }
}