using System;
using System.Threading.Tasks;
using OrleansChess.Common.Events;

namespace OrleansChess.Common {
    public interface IChessHub {
        Task<ISuccessOrErrors<IBoardState>> GetBoardState(string gameId);
        Task<ISuccessOrErrors<BoardState>> PlayerIJoinGame(Guid gameId);
        Task<ISuccessOrErrors<BoardState>> PlayerIIJoinGame(Guid gameId);

        Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMove (Guid gameId, string originalPosition, string newPosition, string eTag);
        Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMove (Guid gameId, string originalPosition, string newPosition, string eTag);
        Task OnDisconnectedAsync(Exception exception);
    }
}