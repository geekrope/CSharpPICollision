namespace CSharpPICollision
{
    /// <summary>
    /// Represents 1d segment
    /// </summary>
    /// <param name="Point1">Segment start</param>
    /// <param name="Point2">Segment end</param>
    readonly record struct Segment(double Point1, double Point2);
    /// <summary>
    /// Represents time to collision of two <seealso cref="PhysicalObject"/>
    /// </summary>
    /// <param name="Object1">First interacting object</param>
    /// <param name="Object2">Second interacting object</param>
    /// <param name="Time">Time to collision</param>
    readonly record struct Collision(PhysicalObject Object1, PhysicalObject Object2, double Time);
    /// <summary>
    /// Represents <seealso cref="PhysicalObject"/> properties change after interaction
    /// </summary>
    /// <param name="Position">Position after collision</param>
    /// <param name="Velocity">Velocity after collision</param>    
    readonly record struct CollisionResponse(double Position, double Velocity);
    /// <summary>
    /// Represents horizontal plane properties
    /// </summary>
    /// <param name="Y">Y coordinate of axis</param>
    record class HorizontalAxis(double Y);

    /// <summary>
    /// Invoked when sender collides with another object
    /// </summary>
    /// <param name="sender">Collided object</param>
    delegate void CollideHandler(PhysicalObject sender);

    /// <summary>
    /// Pepresent tsrget of <see cref="CollisionResponse"/>>
    /// </summary>
    enum CollisionTarget : int
    {
        First = 0,
        Second = 1
    }
}
