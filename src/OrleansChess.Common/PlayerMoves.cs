using ChessDotNet;

namespace OrleansChess.Common { 
    public interface IPlayerMove {
        string OriginalPosition { get; set; }
        string NewPosition { get; set; }
        char? Promotion { get; set; }
    }

    public class PlayerMove {
        public string OriginalPosition { get; set; }
        public string NewPosition { get; set; }
        public char? Promotion { get; set; }
    }

    public class PlayerIMove : PlayerMove, IPlayerMove {
        public PlayerIMove() {}
        public PlayerIMove(string originalPosition, string newPosition){
            OriginalPosition = originalPosition;
            NewPosition = newPosition;
        }

        public PlayerIMove(string originalPosition, string newPosition, char? promotion){
            OriginalPosition = originalPosition;
            NewPosition = newPosition;
            Promotion = promotion;
        }
    }

    public class PlayerIIMove : PlayerMove, IPlayerMove {

        public PlayerIIMove() {}
        public PlayerIIMove(string originalPosition, string newPosition){
            OriginalPosition = originalPosition;
            NewPosition = newPosition;
        }

        public PlayerIIMove(string originalPosition, string newPosition, char? promotion){
            OriginalPosition = originalPosition;
            NewPosition = newPosition;
            Promotion = promotion;
        }
    }
}