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
        public async Task OnGameCreation_ShouldNot_AllowBlackMove () {
            var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
            await game.WhiteJoinGame (Guid.NewGuid ());
            await game.BlackJoinGame (Guid.NewGuid ());
            var eTag = await game.GetShortFen().ContinueWith(x=>x.Result.ETag);
            await game.BlackMove ("A7", "A5", eTag).ContinueWith (x => {
                x.Result.WasSuccessful.Should ().BeFalse ();
                x.Result.Data.Should ().BeNull ();
                x.Result.Errors.Should ().NotBeNullOrEmpty ();
            });
        }

        [Fact]
        public async Task OnGameCreation_Should_AllowWhiteMovePawn () {
            var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
            await game.WhiteJoinGame (Guid.NewGuid ());
            var eTag = await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith(x=>x.Result.Data.ETag);
            await game.WhiteMove ("A2", "A4", eTag).ContinueWith (x => {
                x.Result.WasSuccessful.Should ().BeTrue ();
                x.Result.Data.Value.Should ().Be ("rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR");
                x.Result.Errors.Should ().BeNull ();
            });
        }

        [Fact]
        public async Task AfterWhiteMovePawn_Should_AllowBlackMovePawn () {
            var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
            await game.WhiteJoinGame (Guid.NewGuid ());
            var eTag = await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith(x=>x.Result.Data.ETag);;
            eTag = await game.WhiteMove ("A2", "A4", eTag).ContinueWith(x=>x.Result.Data.ETag);
            await game.BlackMove ("A7", "A5", eTag).ContinueWith (x => {
                x.Result.WasSuccessful.Should ().BeTrue ();
                x.Result.Data.Value.Should ().Be ("rnbqkbnr/1ppppppp/8/p7/P7/8/1PPPPPPP/RNBQKBNR");
                x.Result.Errors.Should ().BeNull ();
            });
        }

        [Fact]
        public async Task AfterWhiteMovePawn_ShouldNot_AllowWhiteMovePawn () {
            var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
            await game.WhiteJoinGame (Guid.NewGuid ());
            var eTag = await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith(x=>x.Result.Data.ETag);
            eTag = await game.WhiteMove ("A2", "A4", eTag).ContinueWith(x=>x.Result.Data.ETag);
            await game.WhiteMove ("B2", "B4", eTag).ContinueWith (x => {
                x.Result.WasSuccessful.Should ().BeFalse ();
                x.Result.Data.Should ().BeNull ();
                x.Result.Errors.Should ().NotBeNullOrEmpty ();
            });
        }

        [Fact]
        public async Task AfterWhiteMovePawnThenBlackMovesPawn_Should_AllowWhiteMovePawn () {
            var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
            await game.WhiteJoinGame (Guid.NewGuid ());
            var eTag = await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith(x=>x.Result.Data.ETag);
            eTag = await game.WhiteMove ("A2", "A4", eTag).ContinueWith(x=>x.Result.Data.ETag);
            eTag = await game.BlackMove ("A7", "A5", eTag).ContinueWith(x=>x.Result.Data.ETag);
            await game.WhiteMove ("B2", "B4", eTag).ContinueWith (x => {
                x.Result.WasSuccessful.Should ().BeTrue ();
                x.Result.Data.Value.Should ().Be ("rnbqkbnr/1ppppppp/8/p7/PP6/8/2PPPPPP/RNBQKBNR");
                x.Result.Errors.Should ().BeNull ();
            });
        }

        [Fact]
        public async Task AfterWhiteMovePawnThenBlackMovesPawn_ShouldNot_AllowBlackMovePawn () {
            var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
            await game.WhiteJoinGame (Guid.NewGuid ());
            var eTag = await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith(x=>x.Result.Data.ETag);
            eTag = await game.WhiteMove ("A2", "A4", eTag).ContinueWith(x=>x.Result.Data.ETag);;
            eTag = await game.BlackMove ("A7", "A5", eTag).ContinueWith(x=>x.Result.Data.ETag);;
            await game.BlackMove ("B7", "B5", eTag).ContinueWith (x => {
                x.Result.WasSuccessful.Should ().BeFalse ();
                x.Result.Data.Should ().BeNull ();
                x.Result.Errors.Should ().NotBeNullOrEmpty ();
            });
        }
    }
}