using System;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.TestKit;
using OrleansChess.Grains.Tests.Extensions;
using Xunit;

namespace OrleansChess.Grains.Tests.GameTests {
    public class ETagTests : TestKitBase {
        [Fact]
        public Task AfterBothPlayersJoin_Should_MoveWithWithCorrectETag () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.WhiteMove ("A2", "A4", eTag).ContinueWith (x => {
                    x.Result.WasSuccessful.Should ().BeTrue ();
                    return x.Result;
                }));
        }

        [Fact]
        public Task AfterBothPlayersJoin_Should_PreventMoveWithWithIncorrectETag () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => {
                    var fakeETag = Guid.NewGuid ().ToString ();
                    return game.WhiteMove ("A2", "A4", fakeETag).ContinueWith (x => {
                        x.Result.WasSuccessful.Should ().BeFalse ();
                        return x.Result;
                    });
                });
        }

        [Fact]
        public Task AfterBothPlayersJoinAndWhiteMoves_Should_MoveWithWithCorrectETag () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.WhiteMove ("A2", "A4", eTag))
                .Then (eTag => game.BlackMove ("A7", "A5", eTag).ContinueWith (x => {
                    x.Result.WasSuccessful.Should ().BeTrue ();
                    return x.Result;
                }));
        }

        [Fact]
        public Task AfterBothPlayersJoinAndWhiteMoves_Should_PreventMoveWithWithIncorrectETag () {
            return Silo.NewGame ().BothPlayersJoinGame (out var game)
                .Then (eTag => game.WhiteMove ("A2", "A4", eTag))
                .Then (eTag => {
                    var fakeETag = Guid.NewGuid ().ToString ();
                    return game.BlackMove ("A7", "A5", fakeETag).ContinueWith (x => {
                        x.Result.WasSuccessful.Should ().BeFalse ();
                        return x.Result;
                    });
                });
        }
    }
}