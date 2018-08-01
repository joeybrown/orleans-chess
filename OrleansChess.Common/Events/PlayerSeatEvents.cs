using System;

namespace OrleansChess.Common.Events {
    public static class Constants {
        public const string PlayerSeatEventStream = "Default";
    }

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