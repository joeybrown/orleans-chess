using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OrleansChess.Common {
    public interface IBoardState {
        string Fen { get; }
        string OriginalPosition { get; }
        string NewPosition { get; }
        string ETag { get; }
        Task<IBoardState> ToTask ();
    }

    public class BoardState : IBoardState {
        public BoardState (IBoardState boardState) {
            Fen = boardState.Fen;
            OriginalPosition = boardState.OriginalPosition;
            NewPosition = boardState.NewPosition;
            ETag = boardState.ETag;
        }

        public BoardState (string fen, string originalPosition, string newPosition, string eTag) {
            Fen = fen;
            OriginalPosition = originalPosition;
            NewPosition = newPosition;
            ETag = eTag;
        }

        public Task<IBoardState> ToTask () => Task.FromResult ((IBoardState) this);

        public string Fen { get; }
        public string OriginalPosition { get; }
        public string NewPosition { get; }
        public string ETag { get; }
    }
}