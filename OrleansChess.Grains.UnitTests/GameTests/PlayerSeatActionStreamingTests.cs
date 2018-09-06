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
        [Fact]
        public async Task OnGameCreationWhenWhiteJoins_Should_StreamEvent () {
            var mockStreamProvider = new Mock<IPlayerSeatStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);
            var gameId = Guid.NewGuid ();
            var game = Silo.AddProbe<IGame> (gameId);
            game.Setup (x => x.GetBoardState ()).Returns (new Common.BoardState ("fen", "orig", "new", "eTag").ToTask ());
            var stream = Silo.AddStreamProbe<WhiteJoinedGame> (gameId, nameof (WhiteJoinedGame));
            
            var whiteId = Guid.NewGuid ();
            var whiteSeat = Silo.CreateGrain<SeatWhite> (id: gameId);
            await whiteSeat.JoinGame (whiteId);

            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.PlayerId == whiteId);
        }

        [Fact]
        public async Task OnGameCreationWhenBlackJoins_Should_StreamEvent () {
            var mockStreamProvider = new Mock<IPlayerSeatStreamProvider> ();
            mockStreamProvider.SetupGet (x => x.Name).Returns ("Default");
            Silo.AddServiceProbe (mockStreamProvider);
            var gameId = Guid.NewGuid ();
            var game = Silo.AddProbe<IGame> (gameId);
            game.Setup (x => x.GetBoardState ()).Returns (new Common.BoardState ("fen", "orig", "new", "eTag").ToTask ());
            var stream = Silo.AddStreamProbe<BlackJoinedGame> (gameId, nameof (BlackJoinedGame));
            
            var blackId = Guid.NewGuid ();
            var blackSeat = Silo.CreateGrain<SeatBlack> (id: gameId);
            await blackSeat.JoinGame (blackId);

            stream.Sends.Should ().Be (1);
            stream.VerifySend (x => x.PlayerId == blackId);
        }
    }
}