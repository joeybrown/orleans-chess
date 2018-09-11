using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface IGame : Orleans.IGrainWithGuidKey {
        Task<IBoardState> GetBoardState ();
        Task<bool> IsValidMove (IPlayerMove move);
        Task<IBoardState> ApplyValidatedMove (IPlayerMove move);
    }
}