using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChessDotNet;
using Orleans;
using Orleans.Providers;
using Orleans.Streams;
using OrleansChess.Common;
using OrleansChess.Common.Events;
using OrleansChess.GrainInterfaces.Chess;

namespace OrleansChess.GrainClasses.Chess {
    public class BoardState : IGrainState {
        public TurnBehaviorStateOption BehaviorState { get; set; } = TurnBehaviorStateOption.White;
        public IList<Move> Moves { get; set; } = new List<Move> ();
        public string ETag { get; set; }
        public object State { get; set; }
        public string OriginalPosition { get; set; }
        public string NewPosition { get; set; }
    }

    [StorageProvider (ProviderName = GrainPersistence.GameStateStore)]
    public partial class Board : Grain<BoardState>, IBoard {
        private string PlayerMoveStreamProvider { get; }

        public override Task OnActivateAsync() {
            Behavior = BehaviorFactory.Build(State.BehaviorState);
            return base.OnActivateAsync();
        }

        public Board (IPlayerMoveStreamProvider playerMoveStreamProvider) {
            PlayerMoveStreamProvider = playerMoveStreamProvider.Name;
        }

        public Task<ISuccessOrErrors<BlackMoved>> BlackMove (string originalPosition, string newPosition, string eTag) {
            return Behavior.BlackMove(this, originalPosition, newPosition, eTag);
        }

        public Task<ISuccessOrErrors<WhiteMoved>> WhiteMove (string originalPosition, string newPosition, string eTag) {
            return Behavior.WhiteMove(this, originalPosition, newPosition, eTag);
        }

        private IBehavior Behavior { get; set; }

        private interface IBehavior {
            TurnBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<BlackMoved>> BlackMove (Board game, string originalPosition, string newPosition, string eTag);
            Task<ISuccessOrErrors<WhiteMoved>> WhiteMove (Board game, string originalPosition, string newPosition, string eTag);
        }

        private static class BehaviorFactory {
            public static IBehavior Build (TurnBehaviorStateOption behaviorState) {
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

        private class WhiteTurn : IBehavior {
            public TurnBehaviorStateOption GetBehavior () => TurnBehaviorStateOption.White;
            public Task<ISuccessOrErrors<BlackMoved>> BlackMove (Board board, string originalPosition, string newPosition, string eTag) => new Error<BlackMoved> ("It is not black's turn.").ToTask ();

            public Task<ISuccessOrErrors<WhiteMoved>> WhiteMove (Board board, string originalPosition, string newPosition, string eTag) {
                async Task<ISuccessOrErrors<WhiteMoved>> WhiteMoveDelegate () {
                    var move = new Move (originalPosition, newPosition, Player.White);
                    var game = board.GrainFactory.GetGrain<IGame> (board.GetPrimaryKey ());
                    var isValid = await game.IsValidMove (move);
                    if (!isValid)
                        return new Error<WhiteMoved> ("Not a valid move");
                    var boardState = await game.ApplyValidatedMove (move);
                    board.Behavior = new BlackTurn ();
                    board.State.BehaviorState = TurnBehaviorStateOption.Black;
                    board.State.ETag = Guid.NewGuid ().ToString ();
                    await board.WriteStateAsync ();
                    var provider = board.GetStreamProvider (board.PlayerMoveStreamProvider);
                    var stream = provider.GetStream<WhiteMoved> (board.GetPrimaryKey (), nameof (WhiteMoved));
                    var whiteMoved = new WhiteMoved (boardState);
                    await stream.OnNextAsync (whiteMoved);
                    return new Success<WhiteMoved> (whiteMoved);
                }
                return CompareETagAndExecute.Go (board.State.ETag, eTag, WhiteMoveDelegate);
            }
        }

        private class BlackTurn : IBehavior {
            public TurnBehaviorStateOption GetBehavior () => TurnBehaviorStateOption.Black;

            public Task<ISuccessOrErrors<BlackMoved>> BlackMove (Board board, string originalPosition, string newPosition, string eTag) {
                async Task<ISuccessOrErrors<BlackMoved>> BlackMoveDelegate () {
                    var move = new Move (originalPosition, newPosition, Player.White);
                    var game = board.GrainFactory.GetGrain<IGame> (board.GetPrimaryKey ());
                    var isValid = await game.IsValidMove (move);
                    if (!isValid)
                        return new Error<BlackMoved> ("Not a valid move");
                    var boardState = (BlackMoved) await game.ApplyValidatedMove (move);
                    board.Behavior = new WhiteTurn ();
                    board.State.BehaviorState = TurnBehaviorStateOption.White;
                    board.State.ETag = Guid.NewGuid ().ToString ();
                    await board.WriteStateAsync ();
                    var provider = board.GetStreamProvider (board.PlayerMoveStreamProvider);
                    var stream = provider.GetStream<BlackMoved> (board.GetPrimaryKey (), nameof (BlackMoved));
                    await stream.OnNextAsync (new BlackMoved (boardState));
                    return new Success<BlackMoved> (boardState);
                }

                return CompareETagAndExecute.Go (board.State.ETag, eTag, BlackMoveDelegate);
            }

            public Task<ISuccessOrErrors<WhiteMoved>> WhiteMove (Board game, string originalPosition, string newPosition, string eTag) => new Error<WhiteMoved> ("It is not white's turn.").ToTask ();
        }

    }
}