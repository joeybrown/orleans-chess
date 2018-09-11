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
            game.Setup (x => x.ApplyValidatedMove (It.IsAny<IPlayerMove> ())).ReturnsAsync (boardState);
            game.Setup (x => x.IsValidMove (It.IsAny<IPlayerMove> ())).ReturnsAsync (true);
            return Silo;
        }

        [Fact]
        public async Task PlayerIMove_Should_StreamEvent () {
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
        public async Task PlayerIIMove_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut (gameId);
            var stream = sut.AddStreamProbe<PlayerIIMoved> (gameId, nameof (PlayerIIMoved));

            var board = sut.CreateGrain<Board> (id: gameId);
            var eTagFromPlayerIMove = await board.PlayerIMove ("A2", "A4", gameId.ToString ()).ContinueWith(x => x.Result.Data.ETag);
            var result = await board.PlayerIIMove ("A7", "A5", eTagFromPlayerIMove);

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
        public async Task PlayerIThenPlayerIIThenPlayerIThenPlayerII_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut (gameId);
            var playerIStream = sut.AddStreamProbe<PlayerIMoved> (gameId, nameof (PlayerIMoved));
            var playerIIStream = sut.AddStreamProbe<PlayerIIMoved> (gameId, nameof (PlayerIIMoved));

            var board = sut.CreateGrain<Board> (id: gameId);
            await board.PlayerIMove ("A2", "A4", gameId.ToString ());
            await board.PlayerIIMove ("A7", "A5", eTag);
            await board.PlayerIMove ("B2", "B4", eTag);
            await board.PlayerIIMove ("B7", "B5", eTag);

            playerIIStream.Sends.Should ().Be (2);
            playerIStream.Sends.Should ().Be (2);
        }
    }
}