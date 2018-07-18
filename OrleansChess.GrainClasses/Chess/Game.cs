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
    public enum TurnBehaviorStateOption {
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
        public TurnBehaviorStateOption TurnBehaviorState { get; set; } = TurnBehaviorStateOption.White;
        public GameBehaviorStateOption BehaviorState { get; set; } = GameBehaviorStateOption.NoPlayersActive;
        public IList<Move> Moves { get; set; } = new List<Move> ();
    }

    [StorageProvider (ProviderName = "GameStateStore")]
    public partial class Game : Grain<GameState>, IGame {
        public Task<ISuccessOrErrors<string>> BlackJoinGame (Guid blackId) => Behavior.BlackJoinGameAsync (this, blackId);

        public Task<ISuccessOrErrors<string>> WhiteJoinGame (Guid whiteId) => Behavior.WhiteJoinGameAsync (this, whiteId);

        public Task<ISuccessOrErrors<string>> WhiteMove (string originalPosition, string newPosition) => Behavior.WhiteMove (this, originalPosition, newPosition);

        public Task<ISuccessOrErrors<string>> BlackMove (string originalPosition, string newPosition) => Behavior.BlackMove (this, originalPosition, newPosition);

        public Task<string> GetShortFen () => Task.FromResult (this.ChessGame.GetFen ().Split ().First ());

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
            Behavior = GameBehaviorFactory.Build (State.BehaviorState);
            TurnBehavior = TurnBehaviorFactory.Build (State.TurnBehaviorState);
            ChessGame = new ChessGame ();
            foreach (var move in State.Moves) {
                ChessGame.ApplyMove (move, true);
            }
            return base.OnActivateAsync ();
        }

        private bool BlackIsActive => State.BlackSeatId != null;
        private bool WhiteIsActive => State.WhiteSeatId != null;

        private interface ITurnBehavior {
            TurnBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<string>> BlackMove (Game game, string originalPosition, string newPosition);
            Task<ISuccessOrErrors<string>> WhiteMove (Game game, string originalPosition, string newPosition);
        }

        private static class TurnBehaviorFactory {
            public static ITurnBehavior Build (TurnBehaviorStateOption behaviorState) {
                switch (behaviorState) {
                    case TurnBehaviorStateOption.White:
                        return new WhiteTurn ();
                    case TurnBehaviorStateOption.Black:
                        return new BlackTurn ();
                    default:
                        throw new NotImplementedException ();
                }
            }
        }

        private class WhiteTurn : ITurnBehavior {
            public TurnBehaviorStateOption GetBehavior () => TurnBehaviorStateOption.White;
            public Task<ISuccessOrErrors<string>> BlackMove (Game game, string originalPosition, string newPosition) => new Error<string> ("It is not black's turn.").ToTask();

            public async Task<ISuccessOrErrors<string>> WhiteMove (Game game, string originalPosition, string newPosition) {
                var move = new Move (originalPosition, newPosition, Player.White);
                var isValid = game.ChessGame.IsValidMove (move);
                if (!isValid)
                    return new Error<string> ("Not a valid move");
                game.ChessGame.ApplyMove (move, true);
                game.TurnBehavior = new BlackTurn ();
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                return new Success<string> (fen);
            }
        }

        private class BlackTurn : ITurnBehavior {
            public TurnBehaviorStateOption GetBehavior () => TurnBehaviorStateOption.Black;

            public async Task<ISuccessOrErrors<string>> BlackMove (Game game, string originalPosition, string newPosition) {
                var move = new Move (originalPosition, newPosition, Player.Black);
                var isValid = game.ChessGame.IsValidMove (move);
                if (!isValid)
                    return new Error<string> ("Not a valid move");
                game.ChessGame.ApplyMove (move, true);
                game.TurnBehavior = new WhiteTurn ();
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                return new Success<string> (fen);
            }

            public Task<ISuccessOrErrors<string>> WhiteMove (Game game, string originalPosition, string newPosition) => new Error<string> ("It is not white's turn.").ToTask();
        }

        private interface IGameBehavior {
            GameBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId);
            Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId);
            Task<ISuccessOrErrors<string>> WhiteMove (Game game, string originalPosition, string newPosition);
            Task<ISuccessOrErrors<string>> BlackMove (Game game, string originalPosition, string newPosition);
        }

        private static class GameBehaviorFactory {
            public static IGameBehavior Build (GameBehaviorStateOption behaviorState) {
                switch (behaviorState) {
                    case GameBehaviorStateOption.NoPlayersActive:
                        return new NoPlayersActive ();
                    case GameBehaviorStateOption.WaitingForBlack:
                        return new WaitingForBlack ();
                    case GameBehaviorStateOption.WaitingForWhite:
                        return new WaitingForWhite ();
                    case GameBehaviorStateOption.GameIsActive:
                        return new GameIsActive ();
                    default:
                        throw new NotImplementedException ();
                }
            }
        }

        private static class CommonBehavior {
            public static Task<ISuccessOrErrors<string>> WhiteAlreadyJoined () => new Error<string> ("White has already joined.").ToTask ();

            public static Task<ISuccessOrErrors<string>> BlackAlreadyJoined () => new Error<string> ("Black has already joined.").ToTask ();
        }

        private class NoPlayersActive : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.NoPlayersActive;

            public async Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) {
                game.Behavior = game.WhiteIsActive ? (IGameBehavior) new GameIsActive () : (IGameBehavior) new WaitingForWhite ();
                game.State.BlackSeatId = blackId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }

            public async Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) {
                game.Behavior = game.BlackIsActive ? (IGameBehavior) new GameIsActive () : (IGameBehavior) new WaitingForBlack ();
                game.State.WhiteSeatId = whiteId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }

            public Task<ISuccessOrErrors<string>> WhiteMove (Game game, string originalPosition, string newPosition) => new Error<string> ("No players active.").ToTask ();

            public Task<ISuccessOrErrors<string>> BlackMove (Game game, string originalPosition, string newPosition) => new Error<string> ("No players active.").ToTask ();
        }

        private class WaitingForBlack : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.WaitingForBlack;

            public async Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) {
                game.Behavior = game.WhiteIsActive ? (IGameBehavior) new GameIsActive () : (IGameBehavior) new WaitingForWhite ();
                game.State.BlackSeatId = blackId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }

            public Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) => CommonBehavior.WhiteAlreadyJoined ();

            public Task<ISuccessOrErrors<string>> WhiteMove (Game game, string originalPosition, string newPosition) => new Error<string> ("Waiting for black to join.").ToTask ();

            public Task<ISuccessOrErrors<string>> BlackMove (Game game, string originalPosition, string newPosition) => new Error<string> ("Waiting for black to join.").ToTask ();
        }

        private class WaitingForWhite : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.WaitingForWhite;

            public Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) => CommonBehavior.BlackAlreadyJoined ();

            public async Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) {
                game.Behavior = game.BlackIsActive ? (IGameBehavior) new GameIsActive () : (IGameBehavior) new WaitingForBlack ();
                game.State.WhiteSeatId = whiteId;
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                ISuccessOrErrors<string> result = new Success<string> (fen);
                return result;
            }
            public Task<ISuccessOrErrors<string>> WhiteMove (Game game, string originalPosition, string newPosition) => new Error<string> ("Waiting for white to join.").ToTask ();

            public Task<ISuccessOrErrors<string>> BlackMove (Game game, string originalPosition, string newPosition) => new Error<string> ("Waiting for white to join.").ToTask ();
        }

        private class GameIsActive : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.GameIsActive;

            public Task<ISuccessOrErrors<string>> BlackJoinGameAsync (Game game, Guid blackId) => CommonBehavior.BlackAlreadyJoined ();

            public Task<ISuccessOrErrors<string>> WhiteJoinGameAsync (Game game, Guid whiteId) => CommonBehavior.WhiteAlreadyJoined ();

            public Task<ISuccessOrErrors<string>> WhiteMove (Game game, string originalPosition, string newPosition) => game.TurnBehavior.WhiteMove (game, originalPosition, newPosition);

            public Task<ISuccessOrErrors<string>> BlackMove (Game game, string originalPosition, string newPosition) => game.TurnBehavior.BlackMove (game, originalPosition, newPosition);
        }
    }
}