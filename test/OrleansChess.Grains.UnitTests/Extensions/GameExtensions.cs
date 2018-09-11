using System;
using System.Threading.Tasks;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;

namespace OrleansChess.Grains.UnitTests.Extensions {
    public static class GameExtensions {
        public static async Task BothPlayersJoinGame (this TestKitSilo silo, Guid guid) {
            var whitePlayerId = Guid.NewGuid();
            var blackPlayerId = Guid.NewGuid();            
            
            var whiteSeat = silo.CreateGrain<GrainClasses.Chess.SeatI>(id: guid);
            await whiteSeat.JoinGame(whitePlayerId);

            var blackSeat = silo.CreateGrain<SeatII>(id: guid);
            await blackSeat.JoinGame(blackPlayerId);
        }
    }
}