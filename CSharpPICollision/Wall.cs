using System;

namespace CSharpPICollision
{
    public class Wall : PhysicalObject
    {
        private decimal _position;

        /// <summary>
        /// Returns position of wall
        /// </summary>
        public decimal Position
        {
            get => _position;
        }
        /// <summary>
        /// Returns velocity equals to zero
        /// </summary>
        public decimal Velocity
        {
            get => 0;
        }

        public override decimal GetPosition(decimal? timeDelta = null)
        {
            return this._position;
        }
        public override decimal GetVelocity()
        {
            return Velocity;
        }
        public override decimal Distance(PhysicalObject obj, decimal? timeDelta = null)
        {
            return obj switch
            {
                Block block => block.Distance(this, timeDelta),
                Wall wall => -wall.Position - _position,
                _ => throw new ArgumentException("Cannot define obj type")
            };
        }
        public override decimal ProcessCollision(PhysicalObject obj)
        {
            return 0;
        }

        /// <summary>
        /// Initializes new instance of Wall
        /// </summary>
        public Wall(decimal position)
        {
            this._position = position;
        }
    }
}
