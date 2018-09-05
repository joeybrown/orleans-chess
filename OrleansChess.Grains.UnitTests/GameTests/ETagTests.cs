using System;
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
        public async Task NewGameAfterWhite_Should_RecieveNewETag () {
            var gameId = Guid.NewGuid ();

            var mockStreamProvider = new Mock<IPlayerMoveStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);
            var stream = Silo.AddStreamProbe<WhiteMoved> (gameId, nameof (WhiteMoved));

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

        // [Fact]
        // public Task AfterBothPlayersJoin_Should_PreventMoveWithWithIncorrectETag () {
        //     return Silo.NewGame ().BothPlayersJoinGame (out var game)
        //         .Then (eTag => {
        //             var fakeETag = Guid.NewGuid ().ToString ();
        //             return game.WhiteMove ("A2", "A4", fakeETag).ContinueWith (x => {
        //                 x.Result.WasSuccessful.Should ().BeFalse ();
        //                 return x.Result;
        //             });
        //         });
        // }

        // [Fact]
        // public Task AfterBothPlayersJoinAndWhiteMoves_Should_MoveWithWithCorrectETag () {
        //     return Silo.NewGame ().BothPlayersJoinGame (out var game)
        //         .Then (eTag => game.WhiteMove ("A2", "A4", eTag))
        //         .Then (eTag => game.BlackMove ("A7", "A5", eTag).ContinueWith (x => {
        //             x.Result.WasSuccessful.Should ().BeTrue ();
        //             return x.Result;
        //         }));
        // }

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