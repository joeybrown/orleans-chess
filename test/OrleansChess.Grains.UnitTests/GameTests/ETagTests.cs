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
    public class ETagTests : TestKitBase {

        private (TestKitSilo sut, Mock<IGame> game, IBoard board) BuildSut(Guid gameId) {
            var mockStreamProvider = new Mock<IPlayerMoveStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);

            var game = Silo.AddProbe<IGame> (gameId);
            game.Setup (x => x.IsValidMove(It.IsAny<IPlayerMove>())).Returns(Task.FromResult(true));
            var board = Silo.CreateGrain<Board> (id: gameId);
            return (Silo, game, board);
        }

        [Fact]
        public async Task NewGameAfterPlayerIMove_Should_RecieveCorrectInformation () {
            var gameId = Guid.NewGuid ();
            var (sut, game, board) = BuildSut(gameId);

            var fen = "test";
            var originalPosition = "A2";
            var newPosition = "A4";
            var resultETag = Guid.NewGuid().ToString();
            IBoardState boardState = new OrleansChess.Common.BoardState(fen, originalPosition, newPosition, resultETag);
            
            game.Setup (x => x.ApplyValidatedMove(It.IsAny<IPlayerMove>())).Returns(Task.FromResult(boardState));
            var result = await board.PlayerIMove("A2", "A4", gameId.ToString());
            
            result.WasSuccessful.Should().BeTrue();
            result.Data.ETag.Should().Be(resultETag);
            result.Data.Fen.Should().Be(fen);
            result.Data.NewPosition.Should().Be(newPosition);
            result.Data.OriginalPosition.Should().Be(originalPosition);
        }

        [Fact]
        public async Task NewGameAfterPlayerIIMove_Should_RecieveCorrectInformation () {
            var gameId = Guid.NewGuid ();
            var (sut, game, board) = BuildSut(gameId);

            const string fenAfterPlayerIMove = "rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR";
            const string playerIOriginalPosition = "A2";
            const string playerINewPosition = "A4";
            var playerIMoveResultETag = Guid.NewGuid().ToString();
            IBoardState boardStateAfterPlayerIMove = new OrleansChess.Common.BoardState(fenAfterPlayerIMove, playerIOriginalPosition, playerINewPosition, playerIMoveResultETag);
            
            const string fenAfterPlayerIIMove = "rnbqkbnr/pp1ppppp/8/2p5/P7/8/1PPPPPPP/RNBQKBNR";
            const string playerIIOriginalPosition = "C7";
            const string playerIINewPosition = "C5";
            var playerIIMoveResultETag = Guid.NewGuid().ToString();
            IBoardState boardStateAfterPlayerIIMove = new OrleansChess.Common.BoardState(fenAfterPlayerIIMove, playerIIOriginalPosition, playerIINewPosition, playerIIMoveResultETag);
            
            game.SetupSequence(x =>
                x.ApplyValidatedMove(It.IsAny<IPlayerMove>()))
                    .Returns(Task.FromResult(boardStateAfterPlayerIMove))
                    .Returns(Task.FromResult(boardStateAfterPlayerIIMove));

            var playerIResult = await board.PlayerIMove(playerIOriginalPosition, playerINewPosition, gameId.ToString());
            var result = await board.PlayerIIMove(playerIIOriginalPosition, playerIINewPosition, playerIResult.Data.ETag);

            result.WasSuccessful.Should().BeTrue();
            result.Data.ETag.Should().Be(playerIIMoveResultETag);
            result.Data.Fen.Should().Be(fenAfterPlayerIIMove);
            result.Data.NewPosition.Should().Be(playerIINewPosition);
            result.Data.OriginalPosition.Should().Be(playerIIOriginalPosition);
        }

        [Fact]
        public async Task NewGameAfterPlayerIIMoveWithWrongETag_Should_ReturnError () {
            var gameId = Guid.NewGuid ();
            var (sut, game, board) = BuildSut(gameId);

            const string fenAfterPlayerIMove = "rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR";
            const string playerIOriginalPosition = "A2";
            const string playerINewPosition = "A4";
            var playerIMoveResultETag = Guid.NewGuid().ToString();
            IBoardState boardStateAfterPlayerIMove = new OrleansChess.Common.BoardState(fenAfterPlayerIMove, playerIOriginalPosition, playerINewPosition, playerIMoveResultETag);
            
            const string fenAfterPlayerIIMove = "rnbqkbnr/pp1ppppp/8/2p5/P7/8/1PPPPPPP/RNBQKBNR";
            const string playerIIOriginalPosition = "C7";
            const string playerIINewPosition = "C5";
            var playerIIMoveResultETag = Guid.NewGuid().ToString();

            IBoardState boardStateAfterPlayerIIMove = new OrleansChess.Common.BoardState(fenAfterPlayerIIMove, playerIIOriginalPosition, playerIINewPosition, playerIIMoveResultETag);
            
            game.SetupSequence(x =>
                x.ApplyValidatedMove(It.IsAny<IPlayerMove>()))
                    .Returns(Task.FromResult(boardStateAfterPlayerIMove))
                    .Returns(Task.FromResult(boardStateAfterPlayerIIMove));

            await board.PlayerIMove(playerIOriginalPosition, playerINewPosition, gameId.ToString());
            var wrongETag = Guid.NewGuid().ToString();
            var result = await board.PlayerIIMove(playerIIOriginalPosition, playerIINewPosition, wrongETag);

            result.WasSuccessful.Should().BeFalse();
        }
    }
}