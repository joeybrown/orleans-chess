using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrleansChess.Common {
    public interface ISuccessOrErrors<T> {
        T Data { get; }
        bool WasSuccessful { get; }
        IEnumerable<string> Errors { get; }
        Task<ISuccessOrErrors<T>> ToTask ();
    }

    public abstract class SuccessOrErrorsAbstract<T> : ISuccessOrErrors<T> {
        public abstract T Data { get; }
        public abstract bool WasSuccessful { get; }

        public abstract IEnumerable<string> Errors { get; }

        public Task<ISuccessOrErrors<T>> ToTask () => Task.FromResult ((ISuccessOrErrors<T>) this);
    }

    public class Success<T> : SuccessOrErrorsAbstract<T> {
        public Success (T data) {
            Data = data;
        }
        public override T Data { get; }

        public override bool WasSuccessful => true;

        public override IEnumerable<string> Errors => null;
    }

    public class Error<T> : SuccessOrErrorsAbstract<T> {
        public Error (IEnumerable<string> errors) {
            Errors = errors;
        }

        public Error (string error) {
            Errors = new [] { error };
        }

        public override T Data => default (T);

        public override bool WasSuccessful => false;

        public override IEnumerable<string> Errors { get; }
    }
}