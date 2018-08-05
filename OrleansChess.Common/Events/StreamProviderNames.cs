namespace OrleansChess.Common {
    public interface IPlayerMoveEventStream {
        string ProviderName { get; set; }
    }

    public class PlayerMoveEventStream : IPlayerMoveEventStream {
        public string ProviderName { get; set; } //"Default" for testing
    }
}