using System;

namespace OrleansChess.Common.Events {
    public interface IPlayerSeatEvent {
        Guid PlayerId { get; }
    }

    public class PlayerTookSeatII : IPlayerSeatEvent {
        public PlayerTookSeatII (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class PlayerTookSeatI: IPlayerSeatEvent {
        public PlayerTookSeatI (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class PlayerLeftSeatI : IPlayerSeatEvent {
        public PlayerLeftSeatI (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class PlayerLeftSeatII : IPlayerSeatEvent {
        public PlayerLeftSeatII (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }
}