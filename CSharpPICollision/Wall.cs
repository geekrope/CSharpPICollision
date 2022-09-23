using System;

namespace CSharpPICollision
{
    class Wall : PhysicalObject
    {
        private double _position;

        /// <summary>
        /// Returns position of wall
        /// </summary>
        public double Position
        {
            get => _position;
        }
        /// <summary>
        /// Returns velocity equals to zero
        /// </summary>
        public double Velocity
        {
            get => 0;
        }

        public override double GetPosition(double? timeDelta = null)
        {
            return this._position;
        }
        public override double GetVelocity()
        {
            return Velocity;
        }
        public override double Distance(PhysicalObject obj, double? timeDelta = null)
        {
            return obj switch
            {
                Block block => block.Distance(this, timeDelta),
                Wall wall => -wall.Position - _position,
                _ => throw new ArgumentException("Cannot define obj type")
            };
        }
        public override double ProcessCollision(PhysicalObject obj)
        {
            return double.NaN;
        }

        /// <summary>
        /// Initializes new instance of Wall
        /// </summary>
        public Wall(double position)
        {
            this._position = position;
        }
    }
}
