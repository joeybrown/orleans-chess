using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChessDotNet;
using Orleans;
using Orleans.Providers;
using OrleansChess.GrainInterfaces.Chess;

namespace OrleansChess.GrainClasses.Chess
{
    public enum GameBehaviorStateOption
    {
        NoPlayersActive,
        WaitingForWhite,
        WaitingForBlack,
        GameIsActive
    }

    public enum GamePlayerTrun
    {
        Black,
        White
    }

    public class GameState
    {
        public Guid BlackSeatId { get; set; }
        public Guid WhiteSeatId { get; set; }

        public GamePlayerTrun Turn { get; set; } = GamePlayerTrun.White;
        public GameBehaviorStateOption BehaviorState { get; set; } = GameBehaviorStateOption.NoPlayersActive;

        public IList<Move> Moves { get; set; } = new List<Move>();

        public MoveType LastMoveType { get; set; }
    }

    [StorageProvider(ProviderName = "GameStateStore")]
    public class Game : Grain<GameState>, IGame
    {
        public Task<string> BlackJoinGame(Guid blackId)
        {
            return Behavior.BlackJoinGame(this, blackId);
        }

        public Task<string> WhiteJoinGame(Guid whiteId)
        {
            return Behavior.WhiteJoinGame(this, whiteId);
        }

        private GameBehavior Behavior { get; set; }
        public ChessGame GameState { get; private set; }

        public override Task OnActivateAsync()
        {
            Behavior = GameBehavior.Build(this, State.BehaviorState);
            GameState = new ChessGame();
            foreach (var move in State.Moves)
            {
                GameState.ApplyMove(move, true);
            }
            return Task.CompletedTask;
        }

        private bool BlackIsActive => State.BlackSeatId != null;
        private bool WhiteIsActive => State.WhiteSeatId != null;

        private abstract class GameBehavior
        {
            public abstract Task<string> BlackJoinGame(Game game, Guid blackId);
            public abstract Task<string> WhiteJoinGame(Game game, Guid whiteId);

            public static GameBehavior Build(Game game, GameBehaviorStateOption behaviorState)
            {
                switch (behaviorState)
                {
                    case GameBehaviorStateOption.NoPlayersActive:
                        return new NoPlayersActive(game);
                    case GameBehaviorStateOption.WaitingForBlack:
                        return new WaitingForBlack(game);
                    case GameBehaviorStateOption.WaitingForWhite:
                        return new WaitingForWhite(game);
                    case GameBehaviorStateOption.GameIsActive:
                        return new GameIsActive(game);
                    default:
                        throw new NotImplementedException();
                }
            }

            private class NoPlayersActive : GameBehavior
            {
                public NoPlayersActive(Game game)
                {
                    game.State.BehaviorState = GameBehaviorStateOption.NoPlayersActive;
                }

                public override Task<string> BlackJoinGame(Game game, Guid blackId)
                {
                    var fen = "new fen";
                    game.Behavior = new WaitingForWhite(game);
                    game.State.BehaviorState = GameBehaviorStateOption.WaitingForWhite;
                    game.State.BlackSeatId = blackId;
                    return Task.FromResult(fen);
                }

                public override Task<string> WhiteJoinGame(Game game, Guid whiteId)
                {
                    var fen = "new fen";
                    game.Behavior = new WaitingForBlack(game);
                    game.State.BehaviorState = GameBehaviorStateOption.WaitingForBlack;
                    game.State.WhiteSeatId = whiteId;
                    return Task.FromResult(fen);
                }
            }

            private class WaitingForBlack : GameBehavior
            {
                public WaitingForBlack(Game game)
                {
                    game.State.BehaviorState = GameBehaviorStateOption.WaitingForBlack;
                }

                public override Task<string> BlackJoinGame(Game game, Guid blackId)
                {
                    var fen = "new fen";
                    game.Behavior = game.WhiteIsActive ? (GameBehavior)new GameIsActive(game) : (GameBehavior)new WaitingForWhite(game);
                    game.State.BlackSeatId = blackId;
                    return Task.FromResult(fen);
                }

                public override Task<string> WhiteJoinGame(Game game, Guid whiteId)
                {
                    // todo
                    throw new InvalidOperationException();
                }
            }

            private class WaitingForWhite : GameBehavior
            {

                public WaitingForWhite(Game game)
                {
                    game.State.BehaviorState = GameBehaviorStateOption.WaitingForWhite;
                }

                public override Task<string> BlackJoinGame(Game game, Guid blackId)
                {
                    // todo
                    throw new InvalidOperationException();
                }

                public override Task<string> WhiteJoinGame(Game game, Guid whiteId)
                {
                    var fen = "new fen";
                    game.Behavior = game.WhiteIsActive ? (GameBehavior)new GameIsActive(game) : (GameBehavior)new WaitingForBlack(game);
                    game.State.WhiteSeatId = whiteId;
                    return Task.FromResult(fen);
                }
            }

            private class GameIsActive : GameBehavior
            {
                public GameIsActive(Game game)
                {
                    game.State.BehaviorState = GameBehaviorStateOption.GameIsActive;
                }

                public override Task<string> BlackJoinGame(Game game, Guid blackId)
                {
                    // todo
                    throw new InvalidOperationException();
                }

                public override Task<string> WhiteJoinGame(Game game, Guid whiteId)
                {
                    // todo
                    throw new InvalidOperationException();
                }
            }
        }
    }
}