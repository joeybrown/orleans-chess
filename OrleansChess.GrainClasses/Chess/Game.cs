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
    public enum GamePlayerTrun {
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
        public GamePlayerTrun Turn { get; set; } = GamePlayerTrun.White;
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

        public Task<string> GetShortFen() =>
            Task.FromResult(this.ChessGame.GetFen().Split().First());

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
        public ChessGame ChessGame { get; set; }

        public override Task OnActivateAsync () {
            Behavior = GameBehaviorFactory.Build (this, State.BehaviorState);
            ChessGame = new ChessGame ();
            foreach (var move in State.Moves) {
                ChessGame.ApplyMove (move, true);
            }
            return base.OnActivateAsync ();
        }

        private bool BlackIsActive => State.BlackSeatId != null;
        private bool WhiteIsActive => State.WhiteSeatId != null;

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
            public NoPlayersActive (Game game) {
                game.State.BehaviorState = GameBehaviorStateOption.NoPlayersActive;
            }

            public async Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) {
                game.Behavior = game.WhiteIsActive ? (IGameBehavior) new GameIsActive (game) : (IGameBehavior) new WaitingForWhite (game);
                game.State.BlackSeatId = blackId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }

            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.NoPlayersActive;

            public async Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) {
                game.Behavior = game.BlackIsActive ? (IGameBehavior) new GameIsActive (game) : (IGameBehavior) new WaitingForBlack (game);
                game.State.WhiteSeatId = whiteId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }
        }

        private class WaitingForBlack : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.WaitingForBlack;

            public WaitingForBlack (Game game) {
                game.State.BehaviorState = GameBehaviorStateOption.WaitingForBlack;
            }

            public async Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) {
                game.Behavior = game.WhiteIsActive ? (IGameBehavior) new GameIsActive (game) : (IGameBehavior) new WaitingForWhite (game);
                game.State.BlackSeatId = blackId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen();
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
                game.State.BehaviorState = GameBehaviorStateOption.WaitingForWhite;
            }

            public Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) =>
                CommonBehavior.BlackAlreadyJoined ();

            public async Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) {
                game.Behavior = game.BlackIsActive ? (IGameBehavior) new GameIsActive (game) : (IGameBehavior) new WaitingForBlack (game);
                game.State.WhiteSeatId = whiteId;
                await game.WriteStateAsync();
                var fen = await game.GetShortFen();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }
        }

        private class GameIsActive : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.GameIsActive;

            public GameIsActive (Game game) {
                game.State.BehaviorState = GameBehaviorStateOption.GameIsActive;
            }

            public Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) =>
                CommonBehavior.BlackAlreadyJoined ();

            public Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) =>
                CommonBehavior.WhiteAlreadyJoined ();
        }
    }
}