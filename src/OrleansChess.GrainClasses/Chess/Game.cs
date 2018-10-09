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

    public class GameState : IGrainState {
        public IList<Move> Moves { get; set; } = new List<Move> ();
        public string ETag { get; set; }
        public object State { get; set; }
        public string OriginalPosition { get; set; }
        public string NewPosition { get; set; }
    }

    [StorageProvider (ProviderName = GrainPersistence.GameStateStore)]
    public class Game : Grain<GameState>, IGame {
        // todo: keep track of game completion status

        public Task<IBoardState> GetBoardState () {
            var fen = this.ChessGame.GetFen ().Split ().First ();
            return new Common.BoardState (fen, State.OriginalPosition, State.NewPosition, State.ETag).ToTask ();
        }

        public ChessGame ChessGame { get; set; }

        public override Task OnActivateAsync () {
            ChessGame = new ChessGame ();
            foreach (var move in State.Moves) {
                ChessGame.ApplyMove (move, true);
            }
            return base.OnActivateAsync ();
        }

        public Task<bool> IsValidMove(IPlayerMove move) =>
            Task.FromResult(ChessGame.IsValidMove (move.ToEngineMove()));

        public async Task<IBoardState> ApplyValidatedMove(IPlayerMove playerMove)
        {
            var move = playerMove.ToEngineMove();
            ChessGame.ApplyMove(move, true);
            State.Moves.Add(move);
            State.ETag = Guid.NewGuid().ToString();
            await WriteStateAsync();
            return await GetBoardState();
        }
    }
}