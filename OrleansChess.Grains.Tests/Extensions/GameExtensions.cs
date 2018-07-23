using System;
using System.Threading.Tasks;
using Orleans.TestKit;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;
using OrleansChess.GrainInterfaces.Chess;

namespace OrleansChess.Grains.Tests.Extensions {
        public static class GameExtensions {
        public static Task<ISuccessOrErrors<IFenWithETag>> BothPlayersJoinGame (this IGame game) => game.WhiteJoinGame (Guid.NewGuid ()).Then (eTag => game.BlackJoinGame (Guid.NewGuid ()));
        public static Task<ISuccessOrErrors<IFenWithETag>> BothPlayersJoinGame (this IGame inputGame, out IGame game) {
            game = inputGame;
            return game.BothPlayersJoinGame ();
        }
        public static IGame NewGame (this TestKitSilo silo) => silo.CreateGrain<Game> (Guid.NewGuid ());
    }
}