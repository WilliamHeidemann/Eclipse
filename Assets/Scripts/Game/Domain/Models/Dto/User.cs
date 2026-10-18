using System;

namespace Game.Domain.Models.Dto
{
    [Serializable]
    public class User
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Id})";
        }
    }
}