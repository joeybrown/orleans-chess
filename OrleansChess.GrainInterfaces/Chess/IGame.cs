using System;
using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.GrainInterfaces.Chess
{
    public interface IGame : Orleans.IGrainWithGuidKey
    {
        Task<ISuccessOrErrors<string>> BlackJoinGame(Guid blackId);
        Task<ISuccessOrErrors<string>> WhiteJoinGame(Guid whiteId);
    }
}