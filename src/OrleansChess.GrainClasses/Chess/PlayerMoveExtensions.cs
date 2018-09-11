using ChessDotNet;
using OrleansChess.Common;

namespace OrleansChess.GrainClasses.Chess {
    public static class PlayerMoveExtnesions {
        public static Move ToEngineMove(this IPlayerMove playerMove){
            switch(playerMove) {
                case PlayerIMove move:
                    return new Move(move.OriginalPosition, move.NewPosition, Player.White, move.Promotion);
                case PlayerIIMove move:
                    return new Move(move.OriginalPosition, move.NewPosition, Player.Black, move.Promotion);
                default:
                    throw new System.Exception("Unknown player move");
            }
        }
    }
}