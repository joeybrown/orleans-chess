using System.Threading.Tasks;
using OrleansChess.Common;

namespace OrleansChess.Grains.Tests.Extensions {
        public static class SuccessOrErrorsExtensions {
        public static Task<string> GetETag (this Task<ISuccessOrErrors<IFenWithETag>> input) => input.ContinueWith (x => x.Result.Data.ETag);
        public static async Task<ISuccessOrErrors<IFenWithETag>> Then (this Task<ISuccessOrErrors<IFenWithETag>> input, GameFunction gameFunc) {
            var eTag = await input.GetETag ();
            return await gameFunc (eTag);
        }
    }
}