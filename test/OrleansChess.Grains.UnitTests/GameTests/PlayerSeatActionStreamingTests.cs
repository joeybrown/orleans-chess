using System;
using System.Threading.Tasks;
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
        public async Task OnGameCreationWhenWhiteJoins_Should_StreamEvent () {
            var gameId = Guid.NewGuid();
            var sut = BuildSut(gameId);
            var stream = sut.AddStreamProbe<WhiteJoinedGame> (gameId, nameof (WhiteJoinedGame));
            
            var whiteId = Guid.NewGuid ();
            var whiteSeat = sut.CreateGrain<SeatWhite> (id: gameId);
            await whiteSeat.JoinGame (whiteId);

            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.PlayerId == whiteId);
        }

        [Fact]
        public async Task OnGameCreationWhenBlackJoins_Should_StreamEvent () {
            var gameId = Guid.NewGuid ();
            var sut = BuildSut(gameId);
            var stream = Silo.AddStreamProbe<BlackJoinedGame> (gameId, nameof (BlackJoinedGame));
            
            var blackId = Guid.NewGuid ();
            var blackSeat = Silo.CreateGrain<SeatBlack> (id: gameId);
            await blackSeat.JoinGame (blackId);

            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.PlayerId == blackId);
        }
    }
}