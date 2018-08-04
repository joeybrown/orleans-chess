using System;

namespace OrleansChess.Common.Events {
    public interface IPlayerSeatEvent {
        Guid PlayerId { get; }
    }

    public class BlackJoinedGame : IPlayerSeatEvent {
        public BlackJoinedGame (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class WhiteJoinedGame: IPlayerSeatEvent {
        public WhiteJoinedGame (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class WhiteLeftGame : IPlayerSeatEvent {
        public WhiteLeftGame (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class BlackLeftGame : IPlayerSeatEvent {
        public BlackLeftGame (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }
}