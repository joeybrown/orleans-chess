using System.Threading.Tasks;

namespace OrleansChess.Common {
    public interface IFenWithETag: IValueWithETag<string> {
        Task<IFenWithETag> ToTask();
    }

    public class FenWithETag: IFenWithETag {
        public FenWithETag(string value, string eTag)
        {
            Value = value;
            ETag = eTag;
        }

        public Task<IFenWithETag> ToTask() => Task.FromResult((IFenWithETag) this);

        public string Value {get;}
        public string ETag {get;}
    }
}