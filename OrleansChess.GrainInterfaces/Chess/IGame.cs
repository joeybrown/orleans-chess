using System.Threading.Tasks;
using ChessDotNet;
using OrleansChess.Common;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface IGame : Orleans.IGrainWithGuidKey {
        Task<IBoardState> GetBoardState ();
        Task<bool> IsValidMove (Move move);
        Task<IBoardState> ApplyValidatedMove (Move move);
    }
}