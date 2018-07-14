using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChessDotNet;
using Orleans;
using Orleans.Providers;
using OrleansChess.GrainInterfaces.Chess;
using OrleansChess.GrainClasses;
using OrleansChess.Common;

namespace OrleansChess.GrainClasses.Chess
{
    public enum GamePlayerTrun
    {
        Black,
        White
    }

    public enum GameBehaviorStateOption {
        NoPlayersActive,
        WaitingForWhite,
        WaitingForBlack,
        GameIsActive
    }

    public class GameState
    {
        public Guid? BlackSeatId { get; set; }
        public Guid? WhiteSeatId { get; set; }

        public GamePlayerTrun Turn { get; set; } = GamePlayerTrun.White;
        public GameBehaviorStateOption BehaviorState { get; set; } = GameBehaviorStateOption.NoPlayersActive;
        public IList<Move> Moves { get; set; } = new List<Move>();
        public MoveType LastMoveType { get; set; }
    }

    [StorageProvider(ProviderName = "GameStateStore")]
    public class Game : Grain<GameState>, IGame
    {
        public Task<ISuccessOrErrors<string>> BlackJoinGame(Guid blackId)
        {
            return Behavior.BlackJoinGameAsync(this, blackId);
        }

        public Task<ISuccessOrErrors<string>> WhiteJoinGame(Guid whiteId)
        {
            return Behavior.WhiteJoinGameAsync(this, whiteId);
        }

        private GameBehavior _behavior {get;set;}

        private GameBehavior Behavior { 
            get {
                return _behavior;
                } 
            set {
                State.BehaviorState = value.GetBehavior();
                _behavior = value;
            } 
        }
        public ChessGame GameState { get; set; }

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
            public abstract GameBehaviorStateOption GetBehavior();
            public abstract Task<ISuccessOrErrors<string>> BlackJoinGameAsync(Game game, Guid blackId);
            public abstract Task<ISuccessOrErrors<string>> WhiteJoinGameAsync(Game game, Guid whiteId);

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

                public override async Task<ISuccessOrErrors<string>> BlackJoinGameAsync(Game game, Guid blackId)
                {
                    var fen = "new fen";
                    game.Behavior = game.WhiteIsActive ? (GameBehavior) new GameIsActive(game) : (GameBehavior) new WaitingForWhite(game);
                    game.State.BlackSeatId = blackId;
                    await game.WriteStateAsync();
                    ISuccessOrErrors<string> result = new Success<string>(fen);
                    return result;
                }

                public override GameBehaviorStateOption GetBehavior() => GameBehaviorStateOption.NoPlayersActive;

                public override async Task<ISuccessOrErrors<string>> WhiteJoinGameAsync(Game game, Guid whiteId)
                {
                    var fen = "new fen";
                    game.Behavior = game.BlackIsActive ? (GameBehavior) new GameIsActive(game) : (GameBehavior) new WaitingForBlack(game);
                    game.State.WhiteSeatId = whiteId;
                    await game.WriteStateAsync();
                    ISuccessOrErrors<string> result = new Success<string>(fen);
                    return result;
                }
            }

            private class WaitingForBlack : GameBehavior
            {
                public WaitingForBlack(Game game)
                {
                    game.State.BehaviorState = GameBehaviorStateOption.WaitingForBlack;
                }

                public override async Task<ISuccessOrErrors<string>> BlackJoinGameAsync(Game game, Guid blackId)
                {
                    var fen = "new fen";
                    game.Behavior = game.WhiteIsActive ? (GameBehavior) new GameIsActive(game) : (GameBehavior) new WaitingForWhite(game);
                    game.State.BlackSeatId = blackId;
                    await game.WriteStateAsync();
                    ISuccessOrErrors<string> result = new Success<string>(fen);
                    return result;
                }

                public override GameBehaviorStateOption GetBehavior() => GameBehaviorStateOption.WaitingForBlack;

                public override Task<ISuccessOrErrors<string>> WhiteJoinGameAsync(Game game, Guid whiteId)
                {
                    ISuccessOrErrors<string> error = new Error<string>(new []{
                        "White has already joined."
                    });
                    return Task.FromResult(error);
                }
            }

            private class WaitingForWhite : GameBehavior
            {

                public WaitingForWhite(Game game)
                {
                    game.State.BehaviorState = GameBehaviorStateOption.WaitingForWhite;
                }

                public override Task<ISuccessOrErrors<string>> BlackJoinGameAsync(Game game, Guid blackId)
                {
                    ISuccessOrErrors<string> error = new Error<string>(new []{
                        "Black has already joined."
                    });
                    return Task.FromResult(error);
                }

                public override GameBehaviorStateOption GetBehavior() => GameBehaviorStateOption.WaitingForWhite;

                public override Task<ISuccessOrErrors<string>> WhiteJoinGameAsync(Game game, Guid whiteId)
                {
                    var fen = "new fen";
                    game.Behavior = game.BlackIsActive ? (GameBehavior) new GameIsActive(game) : (GameBehavior) new WaitingForBlack(game);
                    game.State.WhiteSeatId = whiteId;
                    ISuccessOrErrors<string> result = new Success<string>(fen);
                    return Task.FromResult(result);
                }
            }

            private class GameIsActive : GameBehavior
            {
                public GameIsActive(Game game)
                {
                    game.State.BehaviorState = GameBehaviorStateOption.GameIsActive;
                }

                public override Task<ISuccessOrErrors<string>> BlackJoinGameAsync(Game game, Guid blackId)
                {
                    // todo
                    throw new InvalidOperationException();
                }
                
                public override GameBehaviorStateOption GetBehavior() => GameBehaviorStateOption.GameIsActive;

                public override Task<ISuccessOrErrors<string>> WhiteJoinGameAsync(Game game, Guid whiteId)
                {
                    // todo
                    throw new InvalidOperationException();
                }
            }
        }
    }
}