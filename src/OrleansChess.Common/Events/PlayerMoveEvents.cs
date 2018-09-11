using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.Common.Events {
    public class PlayerIIMoved : IBoardState {

        public PlayerIIMoved(IBoardState boardState) {
            Fen = boardState.Fen;
            OriginalPosition = boardState.OriginalPosition;
            NewPosition = boardState.NewPosition;
            ETag = boardState.ETag;
        }

        public PlayerIIMoved (string fen, string originalPosition, string newPosition, string eTag) {
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

    public class PlayerIMoved : IBoardState {
        public PlayerIMoved(IBoardState boardState) {
            Fen = boardState.Fen;
            OriginalPosition = boardState.OriginalPosition;
            NewPosition = boardState.NewPosition;
            ETag = boardState.ETag;
        }

        public PlayerIMoved (string fen, string originalPosition, string newPosition, string eTag) {
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