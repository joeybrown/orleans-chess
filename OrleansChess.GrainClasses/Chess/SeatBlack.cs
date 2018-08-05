using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using OrleansChess.Common;
using OrleansChess.Common.Events;
using OrleansChess.GrainInterfaces.Chess;

namespace OrleansChess.GrainClasses.Chess {
    public class SeatBlackState : IGrainState {
        public Guid? PlayerId { get; set; }
        public SeatBehaviorStateOption BehaviorState { get; set; } = SeatBehaviorStateOption.Unoccupied;
        public string ETag { get; set; }
        public object State { get; set; }
    }

    [StorageProvider (ProviderName = GrainPersistence.SeatBlackStateStore)]
    public class SeatBlack : Grain<SeatBlackState>, ISeatBlack {
        private string PlayerSeatStreamProvider {get;}

        public SeatBlack(IPlayerSeatStreamProvider playerSeatStreamProvider) {
            PlayerSeatStreamProvider = playerSeatStreamProvider.Name;
        }

        public Task<ISuccessOrErrors<Common.BoardState>> JoinGame (Guid playerId) => Behavior.JoinGame (this, playerId);

        public Task<ISuccessOrErrors<Common.BoardState>> LeaveGame () => Behavior.LeaveGame (this);

        public IBehavior Behavior { get; set; }

        public interface IBehavior {
            SeatBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<Common.BoardState>> JoinGame (SeatBlack seat, Guid playerId);
            Task<ISuccessOrErrors<Common.BoardState>> LeaveGame (SeatBlack seat);
        }

        private static class BehaviorFactory {
            public static IBehavior Build (SeatBehaviorStateOption behaviorState) {
                switch (behaviorState) {
                    case SeatBehaviorStateOption.Occupied:
                        return new Occupied ();
                    case SeatBehaviorStateOption.Unoccupied:
                        return new Unoccupied ();
                    default:
                        throw new NotImplementedException ();
                }
            }
        }

        private class Unoccupied : IBehavior {
            public SeatBehaviorStateOption GetBehavior() => SeatBehaviorStateOption.Unoccupied;

            public async Task<ISuccessOrErrors<Common.BoardState>> JoinGame (SeatBlack seat, Guid playerId) {
                seat.State.PlayerId = playerId;
                seat.State.ETag = Guid.NewGuid ().ToString ();
                await seat.WriteStateAsync ();
                var game = seat.GrainFactory.GetGrain<IGame> (seat.GetPrimaryKey ());
                var boardState = await game.GetBoardState ();
                var provider = seat.GetStreamProvider (seat.PlayerSeatStreamProvider);
                var stream = provider.GetStream<BlackJoinedGame> (seat.GetPrimaryKey (), nameof (BlackJoinedGame));
                await stream.OnNextAsync (new BlackJoinedGame (playerId));
                return new Success<Common.BoardState> (new Common.BoardState (boardState));
            }

            public Task<ISuccessOrErrors<Common.BoardState>> LeaveGame (SeatBlack seat) => new Error<Common.BoardState> ("No player at seat").ToTask ();
        }

        private class Occupied : IBehavior {
            public SeatBehaviorStateOption GetBehavior() => SeatBehaviorStateOption.Occupied;

            public Task<ISuccessOrErrors<Common.BoardState>> JoinGame (SeatBlack seat, Guid playerId) => new Error<Common.BoardState> ("There is a player already at seat").ToTask ();

            public async Task<ISuccessOrErrors<Common.BoardState>> LeaveGame (SeatBlack seat) {
                var playerId = seat.State.PlayerId.GetValueOrDefault ();
                seat.State.PlayerId = null;
                seat.State.ETag = Guid.NewGuid ().ToString ();
                await seat.WriteStateAsync ();
                var game = seat.GrainFactory.GetGrain<IGame> (seat.GetPrimaryKey ());
                var boardState = await game.GetBoardState ();
                var provider = seat.GetStreamProvider (seat.PlayerSeatStreamProvider);
                var stream = provider.GetStream<BlackLeftGame> (seat.GetPrimaryKey (), nameof (BlackLeftGame));
                await stream.OnNextAsync (new BlackLeftGame (playerId));
                return new Success<Common.BoardState> (new Common.BoardState (boardState));
            }
        }
    }
}