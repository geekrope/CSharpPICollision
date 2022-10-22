namespace CSharpPICollision
{
    /// <summary>
    /// Mutable implementation of <seealso cref="MaterialPointReadonly"/>
    /// </summary>
    public class MaterialPoint : MaterialPointReadonly
    {
        public double Impulse
        {
            get
            {
                return Mass * Velocity;
            }
        }
        public double Mass
        {
            get; set;
        }
        public double Velocity
        {
            get; set;
        }
        public double Position
        {
            get; set;
        }

        public override string ToString()
        {
            return $"Impulse - {Impulse}, Mass - {Mass}, Velocity - {Velocity}, Position - {Position}";
        }

        /// <summary>
        /// Initializes new instance of MaterialPoint
        /// </summary>
        public MaterialPoint(double mass, double velocity, double position)
        {
            this.Mass = mass;
            this.Velocity = velocity;
            this.Position = position;
        }
    }
}
