using System.Threading.Tasks;
using OrleansChess.Common;
using OrleansChess.Common.Events;

namespace OrleansChess.GrainInterfaces.Chess {
    public interface IBoard : Orleans.IGrainWithGuidCompoundKey {
        Task<ISuccessOrErrors<WhiteMoved>> WhiteMove (string originalPosition, string newPosition, string eTag);
        Task<ISuccessOrErrors<BlackMoved>> BlackMove (string originalPosition, string newPosition, string eTag);
    }
}