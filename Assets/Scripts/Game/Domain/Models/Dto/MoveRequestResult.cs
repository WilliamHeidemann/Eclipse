using System;
using Game.Domain.Models.State;

namespace Game.Domain.Models.Dto
{
    [Serializable]
    public class MoveRequestResult
    {
        public bool WasValid { get; set; }
        public ClientGameState UpdatedGameState { get; set; }
    }
}