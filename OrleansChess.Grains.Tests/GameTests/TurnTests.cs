using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;
using Xunit;

namespace OrleansChess.Grains.Tests.GameTests {
    public class TurnTests : TestKitBase {
        [Fact]
        public Task OnGameCreation_ShouldNot_AllowBlackMove () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.BlackMove ("A7", "A5", eTag).ContinueWith (x => {
                    x.Result.WasSuccessful.Should ().BeFalse ();
                    x.Result.Data.Should ().BeNull ();
                    x.Result.Errors.Should ().NotBeNullOrEmpty ();
                    return x.Result;
                }));
        }

        [Fact]
        public Task OnGameCreation_Should_AllowWhiteMovePawn () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.WhiteMove ("A2", "A4", eTag).ContinueWith (x => {
                    x.Result.WasSuccessful.Should ().BeTrue ();
                    x.Result.Data.Value.Should ().Be ("rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR");
                    x.Result.Errors.Should ().BeNull ();
                    return x.Result;
                }));
        }

        [Fact]
        public Task AfterWhiteMovePawn_Should_AllowBlackMovePawn () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.WhiteMove ("A2", "A4", eTag))
                .Then (eTag => game.BlackMove ("A7", "A5", eTag).ContinueWith (x => {
                    x.Result.WasSuccessful.Should ().BeTrue ();
                    x.Result.Data.Value.Should ().Be ("rnbqkbnr/1ppppppp/8/p7/P7/8/1PPPPPPP/RNBQKBNR");
                    x.Result.Errors.Should ().BeNull ();
                    return x.Result;
                }));
        }

        [Fact]
        public Task AfterWhiteMovePawn_ShouldNot_AllowWhiteMovePawn () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.WhiteMove ("A2", "A4", eTag))
                .Then (eTag => game.WhiteMove ("B2", "B4", eTag).ContinueWith (x => {
                    x.Result.WasSuccessful.Should ().BeFalse ();
                    x.Result.Data.Should ().BeNull ();
                    x.Result.Errors.Should ().NotBeNullOrEmpty ();
                    return x.Result;
                }));
        }

        [Fact]
        public Task AfterWhiteMovePawnThenBlackMovesPawn_Should_AllowWhiteMovePawn () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.WhiteMove ("A2", "A4", eTag))
                .Then (eTag => game.BlackMove ("A7", "A5", eTag))
                .Then (eTag => game.WhiteMove ("B2", "B4", eTag).ContinueWith (x => {
                    x.Result.WasSuccessful.Should ().BeTrue ();
                    x.Result.Data.Value.Should ().Be ("rnbqkbnr/1ppppppp/8/p7/PP6/8/2PPPPPP/RNBQKBNR");
                    x.Result.Errors.Should ().BeNull ();
                    return x.Result;
                }));
        }

        [Fact]
        public Task AfterWhiteMovePawnThenBlackMovesPawn_ShouldNot_AllowBlackMovePawn () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.WhiteMove ("A2", "A4", eTag))
                .Then (eTag => game.BlackMove ("A7", "A5", eTag))
                .Then (eTag => game.BlackMove ("B7", "B5", eTag).ContinueWith (x => {
                    x.Result.WasSuccessful.Should ().BeFalse ();
                    x.Result.Data.Should ().BeNull ();
                    x.Result.Errors.Should ().NotBeNullOrEmpty ();
                    return x.Result;
                }));
        }
    }
}