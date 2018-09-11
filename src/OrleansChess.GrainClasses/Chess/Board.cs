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
        public TurnBehaviorStateOption BehaviorState { get; set; } = TurnBehaviorStateOption.PlayerI;
        public string ETag { get; set; }
        public object State { get; set; }
        public string OriginalPosition { get; set; }
        public string NewPosition { get; set; }
        public Color PlayerIColor { get; set; } = new Color("red");
        public Color PlayerIIColor { get; set; } = new Color("blue");
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

        public Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMove (string originalPosition, string newPosition, string eTag) {
            return Behavior.PlayerIIMove(this, originalPosition, newPosition, eTag);
        }

        public Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMove (string originalPosition, string newPosition, string eTag) {
            return Behavior.PlayerIMove(this, originalPosition, newPosition, eTag);
        }

        private IBehavior Behavior { get; set; }

        private interface IBehavior {
            TurnBehaviorStateOption GetBehavior ();
            Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMove (Board game, string originalPosition, string newPosition, string eTag);
            Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMove (Board game, string originalPosition, string newPosition, string eTag);
        }

        private static class BehaviorFactory {
            public static IBehavior Build (TurnBehaviorStateOption behaviorState) {
                switch (behaviorState) {
                    case TurnBehaviorStateOption.PlayerI:
                        return new PlayerITurn ();
                    case TurnBehaviorStateOption.PlayerII:
                        return new PlayerIITurn ();
                    default:
                        throw new NotImplementedException ();
                }
            }
        }

        private class PlayerITurn : IBehavior {
            public TurnBehaviorStateOption GetBehavior () => TurnBehaviorStateOption.PlayerI;
            public Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMove (Board board, string originalPosition, string newPosition, string eTag) {
                var color = board.State.PlayerIIColor.FriendlyName;
                return new Error<PlayerIIMoved> ($"It is not {color}'s turn.").ToTask ();
            }

            public Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMove (Board board, string originalPosition, string newPosition, string eTag) {
                async Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMoveDelegate () {
                    var move = new PlayerIMove (originalPosition, newPosition);
                    var game = board.GrainFactory.GetGrain<IGame> (board.GetPrimaryKey ());
                    var isValid = await game.IsValidMove (move);
                    if (!isValid)
                        return new Error<PlayerIMoved> ("Not a valid move");
                    var boardState = await game.ApplyValidatedMove (move);
                    board.Behavior = new PlayerIITurn ();
                    board.State.BehaviorState = TurnBehaviorStateOption.PlayerII;
                    board.State.ETag = boardState.ETag;
                    await board.WriteStateAsync ();
                    var provider = board.GetStreamProvider (board.PlayerMoveStreamProvider);
                    var stream = provider.GetStream<PlayerIMoved> (board.GetPrimaryKey (), nameof (PlayerIMoved));
                    var playerIMoved = new PlayerIMoved (boardState);
                    await stream.OnNextAsync (playerIMoved);
                    return new Success<PlayerIMoved> (playerIMoved);
                }
                return CompareETagAndExecute.Go (board.State.ETag, eTag, PlayerIMoveDelegate);
            }
        }

        private class PlayerIITurn : IBehavior {
            public TurnBehaviorStateOption GetBehavior () => TurnBehaviorStateOption.PlayerII;

            public Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMove (Board board, string originalPosition, string newPosition, string eTag) {
                async Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMovedDelegate () {
                    var move = new PlayerIIMove (originalPosition, newPosition);
                    var game = board.GrainFactory.GetGrain<IGame> (board.GetPrimaryKey ());
                    var isValid = await game.IsValidMove (move);
                    if (!isValid)
                        return new Error<PlayerIIMoved> ("Not a valid move");
                    var boardState = await game.ApplyValidatedMove (move);
                    board.Behavior = new PlayerITurn ();
                    board.State.BehaviorState = TurnBehaviorStateOption.PlayerI;
                    board.State.ETag = boardState.ETag;
                    await board.WriteStateAsync ();
                    var provider = board.GetStreamProvider (board.PlayerMoveStreamProvider);
                    var stream = provider.GetStream<PlayerIIMoved> (board.GetPrimaryKey (), nameof (PlayerIIMoved));
                    var playerIIMoved = new PlayerIIMoved (boardState);
                    await stream.OnNextAsync (playerIIMoved);
                    return new Success<PlayerIIMoved> (playerIIMoved);
                }

                return CompareETagAndExecute.Go (board.State.ETag, eTag, PlayerIIMovedDelegate);
            }

            public Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMove (Board board, string originalPosition, string newPosition, string eTag) {
                var color = board.State.PlayerIIColor.FriendlyName;
                return new Error<PlayerIMoved> ($"It is not {color}'s turn.").ToTask ();
            }
        }

    }
}