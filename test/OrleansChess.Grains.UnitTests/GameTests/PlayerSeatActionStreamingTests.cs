using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.Common.Events;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;
using Xunit;

namespace OrleansChess.Grains.UnitTests.GameTests {
    public class PlayerSeatActionStreamingTests : TestKitBase {

        private TestKitSilo BuildSut(Guid gameId) {
            var mockStreamProvider = new Mock<IPlayerSeatStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);
            var game = Silo.AddProbe<IGame> (gameId);
            game.Setup (x => x.GetBoardState ()).Returns (new Common.BoardState ("fen", "orig", "new", "eTag").ToTask ());
            return Silo;
        }

        [Fact]
        public async Task OnGameCreationWhenPlayerIJoins_Should_StreamEvent () {
            var gameId = Guid.NewGuid();
            var sut = BuildSut(gameId);
            var stream = sut.AddStreamProbe<PlayerTookSeatI> (gameId, nameof (PlayerTookSeatI));
            
            var playerIId = Guid.NewGuid ();
            var playerISeat = sut.CreateGrain<GrainClasses.Chess.SeatI> (id: gameId);
            await playerISeat.JoinGame (playerIId);

            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.PlayerId == playerIId);
        }

        [Fact]
        public async Task OnGameCreationWhenPlayerIIJoins_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut(gameId);
            var stream = Silo.AddStreamProbe<PlayerTookSeatII> (gameId, nameof (PlayerTookSeatII));
            
            var playerId = Guid.NewGuid ();
            var playerSeat = Silo.CreateGrain<SeatII> (id: gameId);
            await playerSeat.JoinGame (playerId);

            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.PlayerId == playerId);
        }
    }
}