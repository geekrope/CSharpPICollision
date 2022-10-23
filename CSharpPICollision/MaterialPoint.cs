namespace CSharpPICollision
{
    /// <summary>
    /// Mutable implementation of <seealso cref="MaterialPointReadonly"/>
    /// </summary>
    public class MaterialPoint : MaterialPointReadonly
    {
        public decimal Impulse
        {
            get
            {
                return Mass * Velocity;
            }
        }
        public decimal Mass
        {
            get; set;
        }
        public decimal Velocity
        {
            get; set;
        }
        public decimal Position
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
        public MaterialPoint(decimal mass, decimal velocity, decimal position)
        {
            this.Mass = mass;
            this.Velocity = velocity;
            this.Position = position;
        }
    }
}
