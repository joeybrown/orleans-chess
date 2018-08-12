// using System;
// using System.Threading.Tasks;
// using FluentAssertions;
// using Orleans.TestKit;
// using OrleansChess.Common;
// using OrleansChess.GrainClasses.Chess;
// using OrleansChess.GrainInterfaces.Chess;
// using Xunit;

// namespace OrleansChess.Grains.Tests.GameTests {
//     public class PlayerSeatActionTests : TestKitBase {
//         [Fact]
//         public async Task OnGameCreation_Should_AllowWhite () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//         }

//         [Fact]
//         public async Task OnGameCreation_Should_AllowBlack () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//         }

//         [Fact]
//         public async Task AfterWhiteJoin_ShouldNot_AllowWhite () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeFalse ());
//         }

//         [Fact]
//         public async Task AfterBlackJoin_ShouldNot_AllowBlack () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());;
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeFalse ());
//         }

//         [Fact]
//         public async Task AfterWhiteJoin_Should_AllowBlack () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//         }

//         [Fact]
//         public async Task AfterBlackJoin_Should_AllowWhite () {
//             var game = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await game.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await game.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//         }

//         [Fact]
//         public async Task AfterBlackAndWhiteJoin_ShouldNot_AllowWhiteOrBlack () {
//             var grain = Silo.CreateGrain<Game> (Guid.NewGuid ());
//             await grain.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await grain.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeTrue ());
//             await grain.BlackJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeFalse ());
//             await grain.WhiteJoinGame (Guid.NewGuid ()).ContinueWith (x => x.Result.WasSuccessful.Should ().BeFalse ());
//         }
//     }
// }