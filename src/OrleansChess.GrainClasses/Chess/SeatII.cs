using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using OrleansChess.Common;
using OrleansChess.Common.Events;
using OrleansChess.GrainInterfaces.Chess;

namespace OrleansChess.GrainClasses.Chess {
    public class SeatIIState : IGrainState {
        public Guid? PlayerId { get; set; }
        public SeatBehaviorStateOption BehaviorState { get; set; } = SeatBehaviorStateOption.Unoccupied;
        public string ETag { get; set; }
        public object State { get; set; }
    }

    [StorageProvider (ProviderName = GrainPersistence.SeatIIStateStore)]
    public class SeatII : Grain<SeatIIState>, ISeatII {
        private string PlayerSeatStreamProvider {get;}

        public SeatII(IPlayerSeatStreamProvider playerSeatStreamProvider) {
            PlayerSeatStreamProvider = playerSeatStreamProvider.Name;
        }

        public override Task OnActivateAsync() {
            Behavior = BehaviorFactory.Build(this.State.BehaviorState);
            return base.OnActivateAsync();
        }

        public Task<ISuccessOrErrors<Common.BoardState>> JoinGame (Guid playerId) => Behavior.JoinGame (this, playerId);

        public Task<ISuccessOrErrors<Common.BoardState>> LeaveGame () => Behavior.LeaveGame (this);

        public IBehavior Behavior { get; set; }

        public interface IBehavior {
            SeatBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<Common.BoardState>> JoinGame (SeatII seat, Guid playerId);
            Task<ISuccessOrErrors<Common.BoardState>> LeaveGame (SeatII seat);
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

            public async Task<ISuccessOrErrors<Common.BoardState>> JoinGame (SeatII seat, Guid playerId) {
                seat.State.PlayerId = playerId;
                seat.State.ETag = Guid.NewGuid ().ToString ();
                await seat.WriteStateAsync ();
                var game = seat.GrainFactory.GetGrain<IGame> (seat.GetPrimaryKey());
                var boardState = await game.GetBoardState ();
                var provider = seat.GetStreamProvider (seat.PlayerSeatStreamProvider);
                var stream = provider.GetStream<PlayerTookSeatII> (seat.GetPrimaryKey (), nameof (PlayerTookSeatII));
                await stream.OnNextAsync (new PlayerTookSeatII (playerId));
                var result = new Success<Common.BoardState> (new Common.BoardState (boardState));
                return result;
            }

            public Task<ISuccessOrErrors<Common.BoardState>> LeaveGame (SeatII seat) => new Error<Common.BoardState> ("No player at seat").ToTask ();
        }

        private class Occupied : IBehavior {
            public SeatBehaviorStateOption GetBehavior() => SeatBehaviorStateOption.Occupied;

            public Task<ISuccessOrErrors<Common.BoardState>> JoinGame (SeatII seat, Guid playerId) => new Error<Common.BoardState> ("There is a player already at seat").ToTask ();

            public async Task<ISuccessOrErrors<Common.BoardState>> LeaveGame (SeatII seat) {
                var playerId = seat.State.PlayerId.GetValueOrDefault ();
                seat.State.PlayerId = null;
                seat.State.ETag = Guid.NewGuid ().ToString ();
                await seat.WriteStateAsync ();
                var game = seat.GrainFactory.GetGrain<IGame> (seat.GetPrimaryKey ());
                var boardState = await game.GetBoardState ();
                var provider = seat.GetStreamProvider (seat.PlayerSeatStreamProvider);
                var stream = provider.GetStream<PlayerLeftSeatII> (seat.GetPrimaryKey (), nameof (PlayerLeftSeatII));
                await stream.OnNextAsync (new PlayerLeftSeatII (playerId));
                return new Success<Common.BoardState> (new Common.BoardState (boardState));

            }
        }
    }
}