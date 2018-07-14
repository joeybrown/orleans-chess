using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessDotNet;
using Orleans;
using Orleans.Providers;
using OrleansChess.Common;
using OrleansChess.GrainClasses;
using OrleansChess.GrainInterfaces.Chess;

namespace OrleansChess.GrainClasses.Chess {
    public enum GameTurnBehaviorStateOption {
        Black,
        White
    }

    public enum GameBehaviorStateOption {
        NoPlayersActive,
        WaitingForWhite,
        WaitingForBlack,
        GameIsActive
    }

    public class GameState {
        public Guid? BlackSeatId { get; set; }
        public Guid? WhiteSeatId { get; set; }
        public GameTurnBehaviorStateOption TurnBehaviorState { get; set; } = GameTurnBehaviorStateOption.White;
        public GameBehaviorStateOption BehaviorState { get; set; } = GameBehaviorStateOption.NoPlayersActive;
        public IList<Move> Moves { get; set; } = new List<Move> ();
        public MoveType LastMoveType { get; set; }
    }

    [StorageProvider (ProviderName = "GameStateStore")]
    public partial class Game : Grain<GameState>, IGame {
        public Task<ISuccessOrErrors<string>> BlackJoinGame (Guid blackId) {
            return Behavior.BlackJoinGameAsync (this, blackId);
        }

        public Task<ISuccessOrErrors<string>> WhiteJoinGame (Guid whiteId) {
            return Behavior.WhiteJoinGameAsync (this, whiteId);
        }

        public Task<ISuccessOrErrors<string>> WhiteMove() {
            return TurnBehavior.WhiteMove(this);
        }

        public Task<ISuccessOrErrors<string>> BlackMove() {
            return TurnBehavior.WhiteMove(this);
        }

        public Task<string> GetShortFen () =>
            Task.FromResult (this.ChessGame.GetFen ().Split ().First ());

        private IGameBehavior _behavior { get; set; }
        private IGameBehavior Behavior {
            get {
                return _behavior;
            }
            set {
                State.BehaviorState = value.GetBehavior ();
                _behavior = value;
            }
        }

        private ITurnBehavior _turnBehavior { get; set; }
        private ITurnBehavior TurnBehavior {
            get {
                return _turnBehavior;
            }
            set {
                State.TurnBehaviorState = value.GetBehavior ();
                _turnBehavior = value;
            }
        }

        public ChessGame ChessGame { get; set; }

        public override Task OnActivateAsync () {
            Behavior = GameBehaviorFactory.Build (this, State.BehaviorState);
            TurnBehavior = TurnBehaviorFactory.Build (this, State.TurnBehaviorState);
            ChessGame = new ChessGame ();
            foreach (var move in State.Moves) {
                ChessGame.ApplyMove (move, true);
            }
            return base.OnActivateAsync ();
        }

        private bool BlackIsActive => State.BlackSeatId != null;
        private bool WhiteIsActive => State.WhiteSeatId != null;

        private interface ITurnBehavior {
            GameTurnBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<string>> BlackMove (Game game);
            Task<ISuccessOrErrors<string>> WhiteMove (Game game);
        }

        private static class TurnBehaviorFactory {
            public static ITurnBehavior Build (Game game, GameTurnBehaviorStateOption behaviorState) {
                switch (behaviorState) {
                    case GameTurnBehaviorStateOption.White:
                        return new WhiteTurn (game);
                    case GameTurnBehaviorStateOption.Black:
                        return new BlackTurn (game);
                    default:
                        throw new NotImplementedException ();
                }
            }
        }

        private class WhiteTurn : ITurnBehavior {
            public GameTurnBehaviorStateOption GetBehavior () => GameTurnBehaviorStateOption.White;

            public WhiteTurn (Game game) {
                game.State.TurnBehaviorState = GetBehavior ();
                // write state?
            }

            public Task<ISuccessOrErrors<string>> BlackMove (Game game) {
                ISuccessOrErrors<string> error = new Error<string> (new [] {
                    "It is not black's turn."
                });
                return Task.FromResult (error);
            }

            public async Task<ISuccessOrErrors<string>> WhiteMove (Game game) {
                // move
                game.TurnBehavior = new WhiteTurn(game);
                await game.WriteStateAsync();
                var fen = await game.GetShortFen ();
                return new Success<string> (fen);
            }
        }

        private class BlackTurn : ITurnBehavior {
            public GameTurnBehaviorStateOption GetBehavior () => GameTurnBehaviorStateOption.Black;

            public BlackTurn (Game game) {
                game.State.TurnBehaviorState = GetBehavior ();
                // write state?
            }

            public async Task<ISuccessOrErrors<string>> BlackMove (Game game) {
                // move
                game.TurnBehavior = new WhiteTurn(game);
                await game.WriteStateAsync();
                var fen = await game.GetShortFen ();
                return new Success<string> (fen);
            }

            public Task<ISuccessOrErrors<string>> WhiteMove (Game game) {
                ISuccessOrErrors<string> error = new Error<string> (new [] {
                    "It is not white's turn."
                });
                return Task.FromResult (error);
            }
        }

        private interface IGameBehavior {
            GameBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId);
            Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId);
        }

        private static class GameBehaviorFactory {
            public static IGameBehavior Build (Game game, GameBehaviorStateOption behaviorState) {
                switch (behaviorState) {
                    case GameBehaviorStateOption.NoPlayersActive:
                        return new NoPlayersActive (game);
                    case GameBehaviorStateOption.WaitingForBlack:
                        return new WaitingForBlack (game);
                    case GameBehaviorStateOption.WaitingForWhite:
                        return new WaitingForWhite (game);
                    case GameBehaviorStateOption.GameIsActive:
                        return new GameIsActive (game);
                    default:
                        throw new NotImplementedException ();
                }
            }
        }

        private static class CommonBehavior {
            public static Task<ISuccessOrErrors<string>> WhiteAlreadyJoined () {
                ISuccessOrErrors<string> error = new Error<string> (new [] {
                    "White has already joined."
                });
                return Task.FromResult (error);
            }

            public static Task<ISuccessOrErrors<string>> BlackAlreadyJoined () {
                ISuccessOrErrors<string> error = new Error<string> (new [] {
                    "Black has already joined."
                });
                return Task.FromResult (error);
            }
        }

        private class NoPlayersActive : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.NoPlayersActive;

            public NoPlayersActive (Game game) {
                game.State.BehaviorState = GetBehavior ();
                // write state?
            }

            public async Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) {
                game.Behavior = game.WhiteIsActive ? (IGameBehavior) new GameIsActive (game) : (IGameBehavior) new WaitingForWhite (game);
                game.State.BlackSeatId = blackId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }

            public async Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) {
                game.Behavior = game.BlackIsActive ? (IGameBehavior) new GameIsActive (game) : (IGameBehavior) new WaitingForBlack (game);
                game.State.WhiteSeatId = whiteId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }
        }

        private class WaitingForBlack : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.WaitingForBlack;

            public WaitingForBlack (Game game) {
                game.State.BehaviorState = GetBehavior ();
                // write state?
            }

            public async Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) {
                game.Behavior = game.WhiteIsActive ? (IGameBehavior) new GameIsActive (game) : (IGameBehavior) new WaitingForWhite (game);
                game.State.BlackSeatId = blackId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }

            public Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) =>
                CommonBehavior.WhiteAlreadyJoined ();
        }

        private class WaitingForWhite : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () =>
                GameBehaviorStateOption.WaitingForWhite;

            public WaitingForWhite (Game game) {
                game.State.BehaviorState = GetBehavior ();
                // write state?
            }

            public Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) =>
                CommonBehavior.BlackAlreadyJoined ();

            public async Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) {
                game.Behavior = game.BlackIsActive ? (IGameBehavior) new GameIsActive (game) : (IGameBehavior) new WaitingForBlack (game);
                game.State.WhiteSeatId = whiteId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }
        }

        private class GameIsActive : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.GameIsActive;

            public GameIsActive (Game game) {
                game.State.BehaviorState = GetBehavior ();
                // write state?
            }

            public Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) =>
                CommonBehavior.BlackAlreadyJoined ();

            public Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) =>
                CommonBehavior.WhiteAlreadyJoined ();
        }
    }
}