using System;
using System.Threading.Tasks;
using OrleansChess.Common;
using OrleansChess.Common.Events;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface ISeatBlack {
        Task<ISuccessOrErrors<BlackJoinedGame>> BlackJoinGame (Guid blackId);
        Task<ISuccessOrErrors<BlackLeftGame>> BlackLeftGame (Guid blackId);
    }
}