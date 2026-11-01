using System;
using Game.Domain.Models.State;

namespace Game.Domain.Models.Dto
{
    [Serializable]
    public class OngoingMatch
    {
        public string Id { get; set; }
        public string OpponentId { get; set; }
        public long LastMoveUnixMs { get; set; }
        public ClientGameState ClientGameState { get; set; }
    }
}
