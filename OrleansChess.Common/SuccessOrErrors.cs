using System.Collections.Generic;

namespace OrleansChess.Common {
    public interface ISuccessOrErrors<T> {
        T Data { get; }
        bool WasSuccessful { get; }
        IEnumerable<string> Errors { get; }
    }

    public class Success<T> : ISuccessOrErrors<T> {
        public Success (T data) {
            Data = data;
        }
        public T Data { get; }

        public bool WasSuccessful => true;

        public IEnumerable<string> Errors =>
        throw new System.NotImplementedException ();
    }

    public class Error<T> : ISuccessOrErrors<T> {
        public Error (IEnumerable<string> errors) {
            Errors = errors;
        }
        public T Data =>
        throw new System.NotImplementedException ();

        public bool WasSuccessful => false;

        public IEnumerable<string> Errors { get; }
    }
}