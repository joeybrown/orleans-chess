using System;
using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface IGame : Orleans.IGrainWithGuidKey {
        Task<ISuccessOrErrors<string>> BlackJoinGame (Guid blackId);
        Task<ISuccessOrErrors<string>> WhiteJoinGame (Guid whiteId);
        Task<ISuccessOrErrors<string>> WhiteMove (string originalPosition, string newPosition);
        Task<ISuccessOrErrors<string>> BlackMove (string originalPosition, string newPosition);
        Task<string> GetShortFen ();
    }
}