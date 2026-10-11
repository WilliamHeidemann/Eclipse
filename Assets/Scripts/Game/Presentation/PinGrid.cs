using Game.Domain.Models;
using Game.Domain.Models.Organisms;
using UnityEngine;

namespace Game.Presentation
{
    public class PinGrid : MonoBehaviour
    {
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private Vector2 _offset;
    
        public Vector3 Get(Position position)
        {
            return GridPosition((int)position.Column, (int)position.Row);
        }

        private Vector2 GridPosition(int column, int row)
        {
            return _offset + new Vector2(column * _spacing.x, row * _spacing.y);
        }
    
        private void OnDrawGizmos()
        {
            const int rows = 6;
            const int columns = 8;

            Gizmos.color = Color.red;
        
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Vector3 position = new Vector3(_offset.x, _offset.y) + new Vector3(i * _spacing.x, j * _spacing.y);
                    Gizmos.DrawSphere(position, 0.1f);
                }
            }
        }
    }
}
