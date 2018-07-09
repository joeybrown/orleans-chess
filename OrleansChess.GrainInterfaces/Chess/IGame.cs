using System;
using System.Threading.Tasks;

namespace OrleansChess.GrainInterfaces.Chess
{
    public interface IGame : Orleans.IGrainWithGuidKey
    {
        Task<string> BlackJoinGame(Guid blackId);
        Task<string> WhiteJoinGame(Guid whiteId);
    }
}