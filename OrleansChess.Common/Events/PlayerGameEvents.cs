using System;

namespace OrleansChess.Common.Events {
    public static class Constants {
        public const string PlayerActionStream = "Default";
    }

    public interface IPlayerGameEvent {
        Guid PlayerId { get; }
    }

    public class BlackJoinedGame : IPlayerGameEvent {
        public BlackJoinedGame (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class WhiteJoinedGame: IPlayerGameEvent {
        public WhiteJoinedGame (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class WhiteLeftGame : IPlayerGameEvent {
        public WhiteLeftGame (Guid playerId) {
            PlayerId = playerId;
        }
        public Guid PlayerId { get; }
    }

    public class BlackLeftGame : IPlayerGameEvent {
        public BlackLeftGame (Guid playerId) {
            PlayerId = playerId;
        }

        public Guid PlayerId { get; }
    }
}