using System;

namespace CSharpPICollision
{
    /// <summary>
    /// Implementation of <see cref="PhysicalObject"/>
    /// </summary>
    class Block : PhysicalObject, ICloneable
    {
        private MaterialPoint _properties;
        private double _size;
        private int _collisions;

        /// <summary>
        /// Returns physical object as MaterialPoint
        /// </summary>
        public MaterialPointReadonly Properties
        {
            get => this._properties;
        }
        /// <summary>
        /// Returns block size
        /// </summary>
        public double Size
        {
            get => this._size;
        }
        /// <summary>
        /// Returns collisions the number with any other object
        /// </summary>
        public int Collisions
        {
            get => this._collisions;
        }

        public override double GetPosition(double? timeDelta = null)
        {
            if (timeDelta.HasValue)
            {
                return _properties.Position + _properties.Velocity * timeDelta.Value;
            }
            else
            {
                return _properties.Position;
            }
        }
        public override double GetVelocity()
        {
            return _properties.Velocity;
        }
        public override double Distance(PhysicalObject obj, double? timeDelta = null)
        {
            return obj switch
            {
                Block block => Utils.DistanceBeetweenSegments(Utils.GetSegment(GetPosition(timeDelta), this._size), Utils.GetSegment(block.GetPosition(timeDelta), block.Size)),
                Wall wall => Utils.DistanceBeetweenSegmentAndPoint(Utils.GetSegment(GetPosition(timeDelta), this._size), wall.Position),
                _ => throw new ArgumentException("Cannot define obj type")
            };
        }
        public override double ProcessCollision(PhysicalObject obj)
        {
            this._collisions++;

            return obj switch
            {
                Block block => (this._properties.Impulse - block.Properties.Mass * this._properties.Velocity + 2 * block.Properties.Impulse) / (this._properties.Mass + block.Properties.Mass),
                Wall => -_properties.Velocity,
                _ => throw new ArgumentException("Cannot define obj type")
            };
        }

        /// <summary>
        /// Sets block position
        /// </summary>
        /// <param name="value">Position on axis</param>
        public void SetPosition(double value)
        {
            _properties.Position = value;
        }
        /// <summary>
        /// Sets block velocity
        /// </summary>
        /// <param name="value">Velocity projection on axis</param>
        public void SetVelocity(double value)
        {
            _properties.Velocity = value;
        }

        public object Clone()
        {
            return new Block(_size, _properties.Mass, _properties.Velocity, _properties.Position);
        }
        /// <summary>
        /// Initializes new instance of Block
        /// </summary>
        public Block(double size, double mass, double velocity, double position)
        {
            this._properties = new MaterialPoint(mass, velocity, position);
            this._size = size;
            this._collisions = 0;
        }
        public Block(double size, MaterialPoint properties)
        {
            this._properties = new MaterialPoint(properties.Mass, properties.Velocity, properties.Position);
            this._size = size;
            this._collisions = 0;
        }
    }
}
