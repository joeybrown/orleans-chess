using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;
using Xunit;

namespace OrleansChess.Grains.Tests.GameTests {
    public class PlayerSeatActionTests : TestKitBase {

        private TestKitSilo BuildSut(Guid gameId) {
            var mockStreamProvider = new Mock<IPlayerSeatStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);
            var game = Silo.AddProbe<IGame> (gameId);
            game.Setup (x => x.GetBoardState ()).Returns (new Common.BoardState ("fen", "orig", "new", "eTag").ToTask ());
            return Silo;
        }

        [Fact]
        public async Task EmptyWhiteSeat_Should_AllowWhite () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut(gameId);

            var whiteSeat = sut.CreateGrain<SeatWhite> (gameId);
            var playerId = Guid.NewGuid();
            var result = await whiteSeat.JoinGame(playerId);
            result.WasSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task OnGameCreation_Should_AllowBlack () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut(gameId);
            
            var blackSeat = Silo.CreateGrain<SeatBlack> (gameId);
            var playerId = Guid.NewGuid();
            var result = await blackSeat.JoinGame(playerId);
            result.WasSuccessful.Should().BeTrue();
        }

//         [Fact]
//         public async Task AfterWhiteJoin_ShouldNot_AllowWhite () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeFalse ());
//         }

//         [Fact]
//         public async Task AfterBlackJoin_ShouldNot_AllowBlack () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());;
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeFalse ());
//         }

//         [Fact]
//         public async Task AfterWhiteJoin_Should_AllowBlack () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//         }

//         [Fact]
//         public async Task AfterBlackJoin_Should_AllowWhite () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//         }

//         [Fact]
//         public async Task AfterBlackAndWhiteJoin_ShouldNot_AllowWhiteOrBlack () {
//             var grain = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await grain.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await grain.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await grain.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeFalse ());
//             await grain.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeFalse ());
//         }
    }
}