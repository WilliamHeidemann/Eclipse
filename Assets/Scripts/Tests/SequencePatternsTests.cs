using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using Game.Domain.Models;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SequencePatternsTests
    {
        // Note: As SequencePatterns.Around(Position) is a static method, these tests 
        // assume a standard grid/layout where adjacent/linear positions form valid patterns.
        // The specific Position parameters (e.g., coordinates or indices) should be adjusted 
        // to match your concrete Position struct/class and actual SequencePatterns output.

        private Team _activeTeam;
        private Team _opposingTeam;
        private HashSet<Position> _lockedPositions;

        [SetUp]
        public void Setup()
        {
            _activeTeam = Team.Red;
            _opposingTeam = Team.Yellow;
            _lockedPositions = new HashSet<Position>();
        }

        [Test]
        public void Sequences_NoCompletedPatterns_ReturnsEmpty()
        {
            // Arrange
            Position movePos = new(Row.One, Column.One);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            // Board contains only the move just played, not a full sequence
            Move[] boardMoves = { move };
            Board board = new(boardMoves);

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            Assert.That(result, Is.Empty,
                "Expected no sequences when the team does not own all positions in any pattern.");
        }

        [Test]
        public void Sequences_OneCompletedPattern_ReturnsSequence()
        {
            // Arrange
            Position movePos = new(Row.One, Column.One);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            // Assuming (One,One), (One,Two), (One,Three), (One,Four) forms a valid pattern returned by SequencePatterns.Around
            Position[] patternPositions =
            {
                movePos,
                new(Row.One, Column.Two),
                new(Row.One, Column.Three),
                new(Row.One, Column.Four)
            };

            Board board = CreateBoardWithTeamOwnership(patternPositions, _activeTeam);

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.First(), Is.EquivalentTo(patternPositions));
        }
        
        [Test]
        public void Sequences_ThreeInARow_ReturnsNoSequence()
        {
            // Arrange
            Position movePos = new(Row.One, Column.One);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            // Assuming (One,One), (One,Two), (One,Three), (One,Four) forms a valid pattern returned by SequencePatterns.Around
            Position[] patternPositions =
            {
                movePos,
                new(Row.One, Column.Two),
                new(Row.One, Column.Three)
            };

            Board board = CreateBoardWithTeamOwnership(patternPositions, _activeTeam);

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Sequences_PatternContainsOpponentPieces_IsFilteredOut()
        {
            // Arrange
            Position movePos = new(Row.One, Column.One);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            Position[] patternPositions =
            {
                movePos,
                new(Row.One, Column.Two),
                new(Row.One, Column.Three),
                new(Row.One, Column.Four)
            };

            // Active team owns 3, Opponent owns 1
            Move[] moves =
            {
                new(patternPositions[0], CreateDummyCard(), _activeTeam),
                new(patternPositions[1], CreateDummyCard(), _activeTeam),
                new(patternPositions[2], CreateDummyCard(), _activeTeam),
                new(patternPositions[3], CreateDummyCard(), _opposingTeam)
            };

            Board board = new(moves);

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Sequences_OneLockedPositionInPattern_KeepsSequence()
        {
            // Arrange
            Position movePos = new(Row.One, Column.One);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            Position[] patternPositions =
            {
                movePos,
                new(Row.One, Column.Two),
                new(Row.One, Column.Three),
                new(Row.One, Column.Four)
            };

            Board board = CreateBoardWithTeamOwnership(patternPositions, _activeTeam);
            _lockedPositions.Add(patternPositions[1]); // Exactly 1 locked position

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            Assert.That(result, Is.Not.Empty, "Sequence should be kept if less than 2 positions are locked.");
        }

        [Test]
        public void Sequences_TwoLockedPositionsInPattern_FiltersOutSequence()
        {
            // Arrange
            Position movePos = new(Row.One, Column.One);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            Position[] patternPositions =
            {
                movePos,
                new(Row.One, Column.Two),
                new(Row.One, Column.Three),
                new(Row.One, Column.Four)
            };

            Board board = CreateBoardWithTeamOwnership(patternPositions, _activeTeam);
            _lockedPositions.Add(patternPositions[1]);
            _lockedPositions.Add(patternPositions[2]); // 2 locked positions

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            Assert.That(result, Is.Empty, "Sequence should be filtered out if 2 or more positions are locked.");
        }

        [Test]
        public void Sequences_MultiplePatterns_WithOverlapLessThanTwo_MaintainsSeparateSequences()
        {
            // Arrange
            Position movePos = new(Row.Two, Column.Two);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            // Assume these form two distinct intersecting lines (e.g., horizontal and vertical) 
            // intersecting ONLY at the movePos (overlap of Two).
            Position[] pattern1 =
                { new(Row.Two, Column.One), movePos, new(Row.Two, Column.Three), new(Row.Two, Column.Four) };
            Position[] pattern2 =
                { new(Row.One, Column.Two), movePos, new(Row.Three, Column.Two), new(Row.Four, Column.Two) };

            Position[] allPositions = pattern1.Concat(pattern2).Distinct().ToArray();
            Board board = CreateBoardWithTeamOwnership(allPositions, _activeTeam);

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            // Due to the Cartesian product in CombineAll, an overlap < 2 returns seq1 uncombined.
            // We expect the result to contain both patterns separately (and potentially duplicates 
            // based on how CombineAll yields A+A, A+B, B+A, B+B).
            Position[][] distinctSequences = result.Select(seq => seq.OrderBy(p => p.GetHashCode()).ToArray())
                .DistinctBy(seq => string.Join(",", seq))
                .ToArray();

            Assert.That(distinctSequences.Length, Is.EqualTo(2));
            Assert.That(distinctSequences.Any(s => s.Length == 4 && s.Contains(pattern1[0])), Is.True);
            Assert.That(distinctSequences.Any(s => s.Length == 4 && s.Contains(pattern2[0])), Is.True);
            Assert.That(distinctSequences.Any(s => s.Length >= 7), Is.False,
                "Sequences should not be combined if overlap < 2.");
        }

        [Test]
        public void Sequences_MultiplePatterns_WithOverlapOfTwoOrMore_CombinesIntoSingleSequence()
        {
            // Arrange
            Position movePos = new(Row.One, Column.Three);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            Position[] existing =
            {
                new(Row.One, Column.One),
                new(Row.One, Column.Two),
                movePos,
                new(Row.One, Column.Four),
                new(Row.One, Column.Five),
                new(Row.One, Column.Six),
            };

            Board board = CreateBoardWithTeamOwnership(existing, _activeTeam);

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            // The CombineAll logic concatenates and uses Distinct() when overlap >= 2.
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Length, Is.EqualTo(6));
        }

        [Test]
        public void Sequences_ExtendingLockedSequence_DoesNotYieldNewSequence()
        {
            // Arrange
            Position movePos = new(Row.One, Column.Six);
            Move move = new(movePos, CreateDummyCard(), _activeTeam);

            Position[] existing =
            {
                new(Row.One, Column.One),
                new(Row.One, Column.Two),
                new(Row.One, Column.Three),
                new(Row.One, Column.Four),
                new(Row.One, Column.Five),
            };

            foreach (Position position in existing)
            {
                _lockedPositions.Add(position);
            }

            Board board = CreateBoardWithTeamOwnership(existing, _activeTeam);

            // Act
            Position[][] result = SequencePatterns.FindSequences(board, move.Team, _lockedPositions).ToArray();

            // Assert
            Assert.That(result.Length, Is.EqualTo(0));
        }

        #region Helpers

        private Board CreateBoardWithTeamOwnership(IEnumerable<Position> positions, Team team)
        {
            Move[] moves = positions.Select(p => new Move(p, CreateDummyCard(), team)).ToArray();
            return new Board(moves);
        }

        private Card CreateDummyCard()
        {
            // Assuming standard instantiation or a mock. 
            // Adjust parameters as required by your domain's Card constructor.
            return default!;
        }

        #endregion
    }
}