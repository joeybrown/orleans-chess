using System;
using System.Threading.Tasks;
using OrleansChess.Common.Events;

namespace OrleansChess.Common {
    public interface IChessHub {
        Task<ISuccessOrErrors<IBoardState>> GetBoardState(string gameId);
        Task<ISuccessOrErrors<BoardState>> PlayerIJoinGame(string gameId);
        Task<ISuccessOrErrors<BoardState>> PlayerIIJoinGame(string gameId);

        Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMove (Guid gameId, string originalPosition, string newPosition, string eTag);
        Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMove (Guid gameId, string originalPosition, string newPosition, string eTag);
        Task OnDisconnectedAsync(Exception exception);
    }
}