using System.Threading.Tasks;
using OrleansChess.Common;
using OrleansChess.Common.Events;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface IBoard : Orleans.IGrainWithGuidKey {
        Task<ISuccessOrErrors<PlayerIMoved>> PlayerIMove (string originalPosition, string newPosition, string eTag);
        Task<ISuccessOrErrors<PlayerIIMoved>> PlayerIIMove (string originalPosition, string newPosition, string eTag);
    }
}