using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.Grains.UnitTests.Extensions {
        public static class SuccessOrErrorsExtensions {
        public static Task<string> GetETag (this Task<ISuccessOrErrors<IBoardState>> input) => input.ContinueWith (x => x.Result.Data.ETag);
        public static async Task<ISuccessOrErrors<IBoardState>> Then (this Task<ISuccessOrErrors<IBoardState>> input, GameFunction gameFunc) {
            var eTag = await input.GetETag ();
            return await gameFunc (eTag);
        }
    }
}