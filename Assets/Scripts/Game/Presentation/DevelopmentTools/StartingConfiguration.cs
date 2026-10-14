using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using UnityEngine;

namespace Game.Presentation
{
    [CreateAssetMenu(fileName = "Starting Configuration", menuName = "Starting Configuration", order = 0)]
    public class StartingConfiguration : ScriptableObject
    {
        [SerializeField] public Mode Mode;
        [SerializeField] public StartIn StartIn;
        [SerializeField] public bool UseDevelopmentStartingState;
        
        [Header("Player = Red")]
        [SerializeField] private List<SerializedCard> _myCards;
        
        [Header("Bot = Yellow")]
        [SerializeField] private List<SerializedCard> _opponentCards;
        
        [Header("Board State")]
        [SerializeField] private List<SerializedMove> _playedMoves;
        
        public GameState DevelopmentGameState()
        {
            Deck deck = new();

            Card[] redCards = _myCards.Count switch
            {
                < 7 => _myCards.Select(c => c.ToModel()).Concat(Enumerable.Range(0, 7 - _myCards.Count).Select(_ => deck.Draw())).ToArray(),
                _ => _myCards.Select(c => c.ToModel()).ToArray()
            };
            
            Card[] yellowCards = _opponentCards.Count switch
            {
                < 7 => _opponentCards.Select(c => c.ToModel()).Concat(Enumerable.Range(0, 7 - _opponentCards.Count).Select(_ => deck.Draw())).ToArray(),
                _ => _opponentCards.Select(c => c.ToModel()).ToArray()
            };

            Move[] moves = _playedMoves.Select(m => m.ToModel()).ToArray();

            Sequence[] sequences = Array.Empty<Sequence>();

            return new GameState(redCards, yellowCards, deck.GetCards(), moves, sequences, Team.Red);
        }

        [Serializable]
        public class SerializedCard
        {
            [SerializeField] private Rank _rank;
            [SerializeField] private Symbol _symbol;
            public Card ToModel() => new(_symbol, _rank);
        }

        [Serializable]
        public class SerializedPosition
        {
            [SerializeField] private Row _row;
            [SerializeField] private Column _column;
            public Position ToModel() => new(_row, _column);
        }
        
        [Serializable]
        public class SerializedMove
        {
            [SerializeField] private SerializedPosition _serializedPosition;
            [SerializeField] private SerializedCard _serializedCard;
            [SerializeField] private Team _team;
            public Move ToModel() => new(_serializedPosition.ToModel(), _serializedCard.ToModel(), _team);
        }
    }
}