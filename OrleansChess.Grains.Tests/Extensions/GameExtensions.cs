using System;
using System.Threading.Tasks;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;

namespace OrleansChess.Grains.Tests.Extensions {
        public static class GameExtensions {
        public static Task<ISuccessOrErrors<IFenWithETag>> BothPlayersJoinGame (this Game game) => game.WhiteJoinGame (Guid.NewGuid ()).Then (eTag => game.BlackJoinGame (Guid.NewGuid ()));
        public static Task<ISuccessOrErrors<IFenWithETag>> BothPlayersJoinGame (this Game inputGame, out Game game) {
            game = inputGame;
            return game.BothPlayersJoinGame ();
        }
        public static Game NewGame (this TestKitSilo silo) => silo.CreateGrain<Game> (Guid.NewGuid ());
    }
}