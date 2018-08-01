using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessDotNet;
using Orleans;
using Orleans.Providers;
using OrleansChess.Common;
using OrleansChess.Common.Events;
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

    public class GameState : IGrainState {
        public Guid? BlackSeatId { get; set; }
        public Guid? WhiteSeatId { get; set; }
        public TurnBehaviorStateOption TurnBehaviorState { get; set; } = TurnBehaviorStateOption.White;
        public GameBehaviorStateOption BehaviorState { get; set; } = GameBehaviorStateOption.NoPlayersActive;
        public IList<Move> Moves { get; set; } = new List<Move> ();
        public string ETag { get; set; }
        public object State { get; set; }
    }

    [StorageProvider (ProviderName = "GameStateStore")]
    public partial class Game : Grain<GameState>, IGame {
        public Task<ISuccessOrErrors<IFenWithETag>> BlackJoinGame (Guid blackId) => Behavior.BlackJoinGameAsync (this, blackId);

        public Task<ISuccessOrErrors<IFenWithETag>> WhiteJoinGame (Guid whiteId) => Behavior.WhiteJoinGameAsync (this, whiteId);

        public Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (string originalPosition, string newPosition, string eTag) => Behavior.WhiteMove (this, originalPosition, newPosition, eTag);

        public Task<ISuccessOrErrors<IFenWithETag>> BlackMove (string originalPosition, string newPosition, string eTag) => Behavior.BlackMove (this, originalPosition, newPosition, eTag);

        public Task<IFenWithETag> GetShortFen () {
            var fen = this.ChessGame.GetFen ().Split ().First ();
            var eTag = State.ETag;
            var result = new FenWithETag (fen, eTag);
            return result.ToTask ();
        }

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
            Task<ISuccessOrErrors<IFenWithETag>> BlackMove (Game game, string originalPosition, string newPosition, string eTag);
            Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (Game game, string originalPosition, string newPosition, string eTag);
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
            public Task<ISuccessOrErrors<IFenWithETag>> BlackMove (Game game, string originalPosition, string newPosition, string eTag) => new Error<IFenWithETag> ("It is not black's turn.").ToTask ();

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (Game game, string originalPosition, string newPosition, string eTag) {
                async Task<ISuccessOrErrors<IFenWithETag>> WhiteMoveDelegate() {
                    var move = new Move (originalPosition, newPosition, Player.White);
                    var isValid = game.ChessGame.IsValidMove (move);
                    if (!isValid)
                        return new Error<IFenWithETag> ("Not a valid move");
                    game.ChessGame.ApplyMove (move, true);
                    game.TurnBehavior = new BlackTurn ();
                    game.State.ETag = Guid.NewGuid ().ToString();
                    await game.WriteStateAsync ();
                    var fen = await game.GetShortFen ();
                    return new Success<IFenWithETag> (fen);
                }
                
                return CompareETagAndExecute.Go(game.State.ETag, eTag, WhiteMoveDelegate);
            }
        }

        private class BlackTurn : ITurnBehavior {
            public TurnBehaviorStateOption GetBehavior () => TurnBehaviorStateOption.Black;

            public Task<ISuccessOrErrors<IFenWithETag>> BlackMove (Game game, string originalPosition, string newPosition, string eTag) {
                async Task<ISuccessOrErrors<IFenWithETag>> BlackMoveDelegate() {
                    var move = new Move (originalPosition, newPosition, Player.Black);
                    var isValid = game.ChessGame.IsValidMove (move);
                    if (!isValid)
                        return new Error<IFenWithETag> ("Not a valid move");
                    game.ChessGame.ApplyMove (move, true);
                    game.TurnBehavior = new WhiteTurn ();
                    game.State.ETag = Guid.NewGuid ().ToString();
                    await game.WriteStateAsync ();
                    var fen = await game.GetShortFen ();
                    return new Success<IFenWithETag> (fen);
                }
                return CompareETagAndExecute.Go(game.State.ETag, eTag, BlackMoveDelegate);

            }

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (Game game, string originalPosition, string newPosition, string eTag) => new Error<IFenWithETag> ("It is not white's turn.").ToTask ();
        }

        private interface IGameBehavior {
            GameBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<IFenWithETag>> BlackJoinGameAsync (Game game, Guid blackId);
            Task<ISuccessOrErrors<IFenWithETag>> WhiteJoinGameAsync (Game game, Guid whiteId);
            Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (Game game, string originalPosition, string newPosition, string eTag);
            Task<ISuccessOrErrors<IFenWithETag>> BlackMove (Game game, string originalPosition, string newPosition, string eTag);
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
            public static Task<ISuccessOrErrors<IFenWithETag>> WhiteAlreadyJoined () => new Error<IFenWithETag> ("White has already joined.").ToTask ();

            public static Task<ISuccessOrErrors<IFenWithETag>> BlackAlreadyJoined () => new Error<IFenWithETag> ("Black has already joined.").ToTask ();
      
            public static async Task<ISuccessOrErrors<IFenWithETag>> WhiteJoinGameAsync (Game game, Guid whiteId) {
                game.Behavior = game.BlackIsActive ? (IGameBehavior) new GameIsActive () : (IGameBehavior) new WaitingForBlack ();
                game.State.WhiteSeatId = whiteId;
                game.State.ETag = Guid.NewGuid ().ToString();
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                var provider = game.GetStreamProvider(Constants.PlayerSeatEventStream);
                var stream = provider.GetStream<WhiteJoinedGame>(game.GetPrimaryKey(), nameof(WhiteJoinedGame));
                await stream.OnNextAsync(new WhiteJoinedGame(whiteId));
                return new Success<IFenWithETag> (fen);
            }

            public static async Task<ISuccessOrErrors<IFenWithETag>> BlackJoinGameAsync (Game game, Guid blackId) {
                game.Behavior = game.WhiteIsActive ? (IGameBehavior) new GameIsActive () : (IGameBehavior) new WaitingForWhite ();
                game.State.BlackSeatId = blackId;
                game.State.ETag = Guid.NewGuid ().ToString();
                await game.WriteStateAsync ();
                var fen = await game.GetShortFen ();
                var provider = game.GetStreamProvider(Constants.PlayerSeatEventStream);
                var stream = provider.GetStream<BlackJoinedGame>(game.GetPrimaryKey(), nameof(BlackJoinedGame));
                await stream.OnNextAsync(new BlackJoinedGame(blackId));
                return new Success<IFenWithETag> (fen);
            }
      }

        private class NoPlayersActive : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.NoPlayersActive;

            public Task<ISuccessOrErrors<IFenWithETag>> BlackJoinGameAsync (Game game, Guid blackId) => CommonBehavior.BlackJoinGameAsync(game, blackId);

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteJoinGameAsync (Game game, Guid whiteId) => CommonBehavior.WhiteJoinGameAsync(game, whiteId);

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (Game game, string originalPosition, string newPosition, string eTag) => new Error<IFenWithETag> ("No players active.").ToTask ();

            public Task<ISuccessOrErrors<IFenWithETag>> BlackMove (Game game, string originalPosition, string newPosition, string eTag) => new Error<IFenWithETag> ("No players active.").ToTask ();
        }

        private class WaitingForBlack : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.WaitingForBlack;

            public Task<ISuccessOrErrors<IFenWithETag>> BlackJoinGameAsync (Game game, Guid blackId) => CommonBehavior.BlackJoinGameAsync(game, blackId);

             public Task<ISuccessOrErrors<IFenWithETag>> WhiteJoinGameAsync (Game game, Guid whiteId) => CommonBehavior.WhiteAlreadyJoined ();

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (Game game, string originalPosition, string newPosition, string eTag) => new Error<IFenWithETag> ("Waiting for black to join.").ToTask ();

            public Task<ISuccessOrErrors<IFenWithETag>> BlackMove (Game game, string originalPosition, string newPosition, string eTag) => new Error<IFenWithETag> ("Waiting for black to join.").ToTask ();
        }

        private class WaitingForWhite : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.WaitingForWhite;

            public Task<ISuccessOrErrors<IFenWithETag>> BlackJoinGameAsync (Game game, Guid blackId) => CommonBehavior.BlackAlreadyJoined ();

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteJoinGameAsync (Game game, Guid whiteId) => CommonBehavior.WhiteJoinGameAsync(game, whiteId);

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (Game game, string originalPosition, string newPosition, string eTag) => new Error<IFenWithETag> ("Waiting for white to join.").ToTask ();

            public Task<ISuccessOrErrors<IFenWithETag>> BlackMove (Game game, string originalPosition, string newPosition, string eTag) => new Error<IFenWithETag> ("Waiting for white to join.").ToTask ();
        }

        private class GameIsActive : IGameBehavior {
            public GameBehaviorStateOption GetBehavior () => GameBehaviorStateOption.GameIsActive;

            public Task<ISuccessOrErrors<IFenWithETag>> BlackJoinGameAsync (Game game, Guid blackId) => CommonBehavior.BlackAlreadyJoined ();

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteJoinGameAsync (Game game, Guid whiteId) => CommonBehavior.WhiteAlreadyJoined ();

            public Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (Game game, string originalPosition, string newPosition, string eTag) => game.TurnBehavior.WhiteMove (game, originalPosition, newPosition, eTag);

            public Task<ISuccessOrErrors<IFenWithETag>> BlackMove (Game game, string originalPosition, string newPosition, string eTag) => game.TurnBehavior.BlackMove (game, originalPosition, newPosition, eTag);
        }
    }
}