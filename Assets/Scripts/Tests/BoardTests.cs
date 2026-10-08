// using System;
// using Game.Domain.Models;
// using NUnit.Framework;
//
// namespace Tests
// {
//     public class BoardTests
//     {
//         [Test]
//         public void HasSequence_ReturnsTrue_WhenSequenceExists()
//         {
//             Board board = new Board(Array.Empty<Move>());
//             board.TryAdd(new Position(Row.One, Column.One), Team.Red);
//             board.TryAdd(new Position(Row.One, Column.Two), Team.Red);
//             board.TryAdd(new Position(Row.One, Column.Three), Team.Red);
//             board.TryAdd(new Position(Row.One, Column.Four), Team.Red);
//
//             var hasSequence = board.HasSequence(Team.Red);
//             
//             Assert.That(hasSequence, Is.True);
//         }
//         
//         [Test]
//         public void HasSequence_ReturnsFalse_WhenNoSequenceExists()
//         {
//             Board board = new Board(Array.Empty<Move>());
//             board.TryAdd(new Position(Row.One, Column.One), Team.Red); 
//             
//             var hasSequence = board.HasSequence(Team.Red);
//             Assert.That(hasSequence, Is.False);
//         }
//         
//         [Test]
//         public void HasSequence_ReturnsFalse_WhenOnlyOtherTeamHasSequence()
//         {
//             Board board = new Board(Array.Empty<Move>());
//             board.TryAdd(new Position(Row.One, Column.One), Team.Red);
//             board.TryAdd(new Position(Row.One, Column.Two), Team.Red);
//             board.TryAdd(new Position(Row.One, Column.Three), Team.Red);
//             board.TryAdd(new Position(Row.One, Column.Four), Team.Red);
//             
//             var hasSequence = board.HasSequence(Team.Yellow);
//             
//             Assert.That(hasSequence, Is.False);
//         }
//     }
// }