using System;
using System.Threading.Tasks;
using OrleansChess.Common;
using OrleansChess.Common.Events;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface ISeatWhite {
        Task<ISuccessOrErrors<WhiteJoinedGame>> JoinGame (Guid playerId);
        Task<ISuccessOrErrors<WhiteLeftGame>> LeaveGame ();
    }
}