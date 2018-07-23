using System;
using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface IGame : Orleans.IGrainWithGuidKey {
        // Actions
        Task<ISuccessOrErrors<IFenWithETag>> BlackJoinGame (Guid blackId);
        Task<ISuccessOrErrors<IFenWithETag>> BlackLeaveGame ();
        Task<ISuccessOrErrors<IFenWithETag>> WhiteJoinGame (Guid whiteId);
        Task<ISuccessOrErrors<IFenWithETag>> WhiteLeaveGame ();
        Task<ISuccessOrErrors<IFenWithETag>> WhiteMove (string originalPosition, string newPosition, string eTag);
        Task<ISuccessOrErrors<IFenWithETag>> BlackMove (string originalPosition, string newPosition, string eTag);
        
        // Queries
        Task<IFenWithETag> GetShortFen ();
    }
}