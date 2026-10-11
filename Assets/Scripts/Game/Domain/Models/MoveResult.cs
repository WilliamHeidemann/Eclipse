using Game.Domain.Models.State;

namespace Game.Domain.Models
{
    public abstract record MoveResult
    {
        public record Success(GameState UpdatedState) : MoveResult;
        public record Invalid : MoveResult;
    }
}