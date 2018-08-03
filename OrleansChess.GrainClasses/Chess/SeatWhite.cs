using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using OrleansChess.Common;
using OrleansChess.Common.Events;
using OrleansChess.GrainInterfaces.Chess;

namespace OrleansChess.GrainClasses.Chess {
    public class SeatWhiteState : IGrainState {
        public Guid? PlayerId { get; set; }
        public SeatBehaviorStateOption BehaviorState { get; set; } = SeatBehaviorStateOption.Unoccupied;
        public string ETag { get; set; }
        public object State { get; set; }
    }

    [StorageProvider (ProviderName = "SeatWhiteStateStore")]
    public class SeatWhite : Grain<SeatWhiteState>, ISeatWhite {
        public Task<ISuccessOrErrors<WhiteJoinedGame>> JoinGame (Guid playerId) => Behavior.JoinGame (this, playerId);

        public Task<ISuccessOrErrors<WhiteLeftGame>> LeaveGame () => Behavior.LeaveGame (this);

        public IBehavior Behavior { get; set; }

        public interface IBehavior {
            Task<ISuccessOrErrors<WhiteJoinedGame>> JoinGame (SeatWhite seat, Guid playerId);
            Task<ISuccessOrErrors<WhiteLeftGame>> LeaveGame (SeatWhite seat);
        }

        private class Unoccupied : IBehavior {
            public Task<ISuccessOrErrors<WhiteJoinedGame>> JoinGame (SeatWhite seat, Guid playerId) {
                throw new NotImplementedException ();
            }

            public Task<ISuccessOrErrors<WhiteLeftGame>> LeaveGame (SeatWhite seat) => new Error<WhiteLeftGame> ("No player at seat").ToTask ();
        }
    }
}