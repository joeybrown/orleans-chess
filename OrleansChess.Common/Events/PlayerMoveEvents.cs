using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.Common.Events {
    public class BlackMoved : IBoardState {

        public BlackMoved(IBoardState boardState) {
            Fen = boardState.Fen;
            OriginalPosition = boardState.OriginalPosition;
            NewPosition = boardState.NewPosition;
            ETag = ETag;
        }

        public BlackMoved (string fen, string originalPosition, string newPosition, string eTag) {
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

    public class WhiteMoved : IBoardState {
        public WhiteMoved(IBoardState boardState) {
            Fen = boardState.Fen;
            OriginalPosition = boardState.OriginalPosition;
            NewPosition = boardState.NewPosition;
            ETag = boardState.ETag;
        }

        public WhiteMoved (string fen, string originalPosition, string newPosition, string eTag) {
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