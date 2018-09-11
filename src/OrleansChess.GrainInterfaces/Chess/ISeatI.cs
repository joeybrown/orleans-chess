using System;
using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface ISeatI : Orleans.IGrainWithGuidKey {
        Task<ISuccessOrErrors<BoardState>> JoinGame (Guid playerId);
        Task<ISuccessOrErrors<BoardState>> LeaveGame ();
    }
}