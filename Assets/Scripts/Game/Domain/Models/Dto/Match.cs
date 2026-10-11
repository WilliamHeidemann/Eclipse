using System;
using Game.Domain.Models.State;

namespace Game.Domain.Models.Dto
{
    [Serializable]
    public class Match
    {
        public string Id { get; set; }
        public ClientGameState ClientGameState { get; set; }
    }
}