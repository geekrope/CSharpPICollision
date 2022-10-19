namespace CSharpPICollision
{
    /// <summary>
    /// MaterialPoint interface
    /// </summary>
    interface MaterialPointReadonly
    {
        /// <summary>
        /// Returns impulse of material point
        /// </summary>
        decimal Impulse
        {
            get;
        }
        /// <summary>
        /// Returns mass of material point
        /// </summary>
        decimal Mass
        {
            get;
        }
        /// <summary>
        /// Returns velocity of material point
        /// </summary>
        decimal Velocity
        {
            get;
        }
        /// <summary>
        /// Returns position of material point
        /// </summary>
        decimal Position
        {
            get;
        }
    }
}
