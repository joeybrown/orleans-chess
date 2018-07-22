using System;
using System.Threading.Tasks;

namespace OrleansChess.Common {
    public static class CompareETagAndExecute{
        public static async Task<ISuccessOrErrors<T>> Go<T>(string originETag, string providedETag, Func<Task<ISuccessOrErrors<T>>> func) {
            if (string.IsNullOrWhiteSpace(originETag) || originETag == providedETag) {
                return await func();
            }
            else return new Error<T>("Out of Date");
        }
    }
}