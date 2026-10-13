using System;
using System.Collections;
using System.Linq;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace Tests
{
    public class SequenceDetectionTests
    {
        [Test]
        public void WhenMoveCompletesSequence_SequenceIsDetected()
        {
            Position[] positions =
            {
                new(Row.One, Column.One),
                new(Row.Two, Column.One),
                new(Row.Three, Column.One),
                new(Row.Four, Column.One),
            };

            Move[] moves = positions.Select(p => new Move(p, default, default)).ToArray();

            Sequence[] sequences =
            {
                new(positions, default)
            };

            ClientGameState clientGameState = new(moves, Array.Empty<Card>(), sequences, default, false);

            int sequencesFromLastMove = clientGameState.SequencesFromLastMove().ToArray().Length;

            Assert.That(sequencesFromLastMove, Is.EqualTo(1));
        }
    }
}