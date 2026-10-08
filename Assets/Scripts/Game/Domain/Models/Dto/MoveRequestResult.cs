using System;

namespace Game.Domain.Models.Dto
{
    [Serializable]
    public class MoveRequestResult
    {
        public bool WasValid { get; set; }
        public ClientGameState UpdatedGameState { get; set; }
        public int DeltaScore { get; set; }
    }
}