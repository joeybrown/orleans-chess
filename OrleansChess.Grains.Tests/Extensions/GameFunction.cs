using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.Grains.Tests.Extensions {
        public delegate Task<ISuccessOrErrors<IBoardState>> GameFunction (string eTag);
}