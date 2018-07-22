using System;

namespace OrleansChess.Common {
    public interface IValueWithETag<T> {
        T Value { get; }
        string ETag { get; }
    }

    public class ValueWithETag<T>:IValueWithETag<T> {
        public T Value {get;}
        public string ETag { get; }

        public ValueWithETag(T value, string eTag)
        {
            Value = value;
            ETag = eTag;
        }
    }
}