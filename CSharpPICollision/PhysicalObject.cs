using System;

namespace CSharpPICollision
{
    /// <summary>
    /// Physical object interface
    /// </summary>
    abstract class PhysicalObject
    {
        /// <summary>
        /// Returns current velocity of physical object
        /// </summary>
        /// <returns>Object velocity</returns>
        public abstract decimal GetVelocity();
        /// <summary>
        /// Returns position of physical object in specific moment of time 
        /// </summary>
        /// <param name="timeDelta">Moment of time</param>
        /// <returns>Object position on axis</returns>
        public abstract decimal GetPosition(decimal? timeDelta = null);
        /// <summary>
        /// Returns distance between current physical object and another object in specific moment of time
        /// </summary>
        /// <param name="obj">Object which is related to distance</param>
        /// <param name="timeDelta">Moment of time</param>
        /// <returns>Distance between two physical object</returns>
        public abstract decimal Distance(PhysicalObject obj, decimal? timeDelta = null);
        /// <summary>
        /// Returns velocity of current object after interaction with another boject
        /// </summary>
        /// <param name="obj">Object which is involved into interaction</param>
        /// <returns>Velocity of current object</returns>
        public abstract decimal ProcessCollision(PhysicalObject obj);
    }
}
