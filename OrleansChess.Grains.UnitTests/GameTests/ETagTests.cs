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
    public class ETagTests : TestKitBase {
        [Fact]
        public async Task NewGameAfterWhiteMove_Should_RecieveCorrectInformation () {
            var gameId = Guid.NewGuid ();

            var mockStreamProvider = new Mock<IPlayerMoveStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);

            var eTag = Guid.NewGuid().ToString();
            var game = Silo.AddProbe<IGame> (gameId);
            game.Setup (x => x.IsValidMove(It.IsAny<Move>())).Returns(Task.FromResult(true));
            var fen = "test";
            var originalPosition = "A2";
            var newPosition = "A4";
            var resultETag = Guid.NewGuid().ToString();
            IBoardState boardState = new OrleansChess.Common.BoardState(fen, originalPosition, newPosition, resultETag);
            game.Setup (x => x.ApplyValidatedMove(It.IsAny<Move>())).Returns(Task.FromResult(boardState));
            var board = Silo.CreateGrain<Board> (id: gameId);
            var result = await board.WhiteMove("A2", "A4", eTag);
            
            result.WasSuccessful.Should().BeTrue();
            result.Data.ETag.Should().Be(resultETag);
            result.Data.Fen.Should().Be(fen);
            result.Data.NewPosition.Should().Be(newPosition);
            result.Data.OriginalPosition.Should().Be(originalPosition);
        }

        [Fact]
        public async Task NewGameAfterBlackMove_Should_RecieveCorrectInformation () {
            var gameId = Guid.NewGuid ();

            var mockStreamProvider = new Mock<IPlayerMoveStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);

            var eTag = Guid.NewGuid().ToString();
            var game = Silo.AddProbe<IGame> (gameId);
            game.Setup (x => x.IsValidMove(It.IsAny<Move>())).Returns(Task.FromResult(true));
            var board = Silo.CreateGrain<Board> (id: gameId);

            const string fenAfterWhiteMove = "rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR";
            const string whiteOriginalPosition = "A2";
            const string whiteNewPosition = "A4";
            var whiteMoveResultETag = Guid.NewGuid().ToString();
            IBoardState boardStateAfterWhiteMove = new OrleansChess.Common.BoardState(fenAfterWhiteMove, whiteOriginalPosition, whiteNewPosition, whiteMoveResultETag);
            
            const string fenAfterBlackMove = "rnbqkbnr/pp1ppppp/8/2p5/P7/8/1PPPPPPP/RNBQKBNR";
            const string blackOriginalPosition = "C7";
            const string blackNewPosition = "C5";
            var blackMoveResultETag = Guid.NewGuid().ToString();

            IBoardState boardStateAfterBlackMove = new OrleansChess.Common.BoardState(fenAfterBlackMove, blackOriginalPosition, blackNewPosition, blackMoveResultETag);
            
            game.SetupSequence(x =>
                x.ApplyValidatedMove(It.IsAny<Move>()))
                    .Returns(Task.FromResult(boardStateAfterWhiteMove))
                    .Returns(Task.FromResult(boardStateAfterBlackMove));

            var whiteResult = await board.WhiteMove(whiteOriginalPosition, whiteNewPosition, eTag);
            var result = await board.BlackMove(blackOriginalPosition, blackNewPosition, whiteResult.Data.ETag);

            result.WasSuccessful.Should().BeTrue();
            result.Data.ETag.Should().Be(blackMoveResultETag);
            result.Data.Fen.Should().Be(fenAfterBlackMove);
            result.Data.NewPosition.Should().Be(blackNewPosition);
            result.Data.OriginalPosition.Should().Be(blackOriginalPosition);
        }

        [Fact]
        public async Task AfterBothPlayersJoin_Should_PreventMoveWithWithIncorrectETag () {

        }

        // [Fact]
        // public Task AfterBothPlayersJoinAndWhiteMoves_Should_PreventMoveWithWithIncorrectETag () {
        //     return Silo.NewGame ().BothPlayersJoinGame (out var game)
        //         .Then (eTag => game.WhiteMove ("A2", "A4", eTag))
        //         .Then (eTag => {
        //             var fakeETag = Guid.NewGuid ().ToString ();
        //             return game.BlackMove ("A7", "A5", fakeETag).ContinueWith (x => {
        //                 x.Result.WasSuccessful.Should ().BeFalse ();
        //                 return x.Result;
        //             });
        //         });
        // }
    }
}