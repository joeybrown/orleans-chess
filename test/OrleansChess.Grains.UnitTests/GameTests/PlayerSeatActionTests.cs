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
        public async Task EmptyPlayerISeat_Should_AllowPlayerI () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut(gameId);

            var playerISeat = sut.CreateGrain<GrainClasses.Chess.SeatI> (gameId);
            var playerId = Guid.NewGuid();
            var result = await playerISeat.JoinGame(playerId);
            result.WasSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task OnGameCreation_Should_AllowPlayerII () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut(gameId);
            
            var playerIISeat = Silo.CreateGrain<SeatII> (gameId);
            var playerId = Guid.NewGuid();
            var result = await playerIISeat.JoinGame(playerId);
            result.WasSuccessful.Should().BeTrue();
        }
    }
}