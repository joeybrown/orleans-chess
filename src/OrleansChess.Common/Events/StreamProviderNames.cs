namespace OrleansChess.Common {
    public interface IStreamProvider
    {
        string Name {get;}
    }

    public abstract class AbstractStreamProvider: IStreamProvider {
        public AbstractStreamProvider(string providerName)
        {
            Name = providerName;
        }

        public string Name { get; }
    }

    public interface IPlayerMoveStreamProvider : IStreamProvider {
    }

    public class PlayerMoveStreamProvider : AbstractStreamProvider, IPlayerMoveStreamProvider
    {
        public PlayerMoveStreamProvider(string providerName) : base(providerName)
        {
        }
    }

    public interface IPlayerSeatStreamProvider: IStreamProvider {
    }

    public class PlayerSeatStreamProvider : AbstractStreamProvider, IPlayerSeatStreamProvider
    {
        public PlayerSeatStreamProvider(string providerName) : base(providerName)
        {
        }
    }
}