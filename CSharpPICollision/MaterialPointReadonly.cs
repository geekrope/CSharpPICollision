namespace CSharpPICollision
{
    /// <summary>
    /// MaterialPoint interface
    /// </summary>
    public interface MaterialPointReadonly
    {
        /// <summary>
        /// Returns impulse of material point
        /// </summary>
        double Impulse
        {
            get;
        }
        /// <summary>
        /// Returns mass of material point
        /// </summary>
        double Mass
        {
            get;
        }
        /// <summary>
        /// Returns velocity of material point
        /// </summary>
        double Velocity
        {
            get;
        }
        /// <summary>
        /// Returns position of material point
        /// </summary>
        double Position
        {
            get;
        }
    }
}
