namespace CSharpPICollision
{
    /// <summary>
    /// Represents 1d segment
    /// </summary>
    /// <param name="Point1">Segment start</param>
    /// <param name="Point2">Segment end</param>
    readonly record struct Segment(decimal Point1, decimal Point2);
    /// <summary>
    /// Represents time to collision of two <seealso cref="PhysicalObject"/>
    /// </summary>
    /// <param name="Object1">First interacting object</param>
    /// <param name="Object2">Second interacting object</param>
    /// <param name="Time">Time to collision</param>
    readonly record struct Collision(PhysicalObject Object1, PhysicalObject Object2, decimal Time);
    /// <summary>
    /// Represents <seealso cref="PhysicalObject"/> properties change after interaction
    /// </summary>
    /// <param name="Position">Position after collision</param>
    /// <param name="Velocity">Velocity after collision</param>    
    readonly record struct CollisionResponse(decimal Position, decimal Velocity);
    /// <summary>
    /// Represents horizontal plane properties
    /// </summary>
    /// <param name="Y">Y coordinate of axis</param>
    public record class HorizontalAxis(decimal Y);

    /// <summary>
    /// Pepresent target of <see cref="CollisionResponse"/>>
    /// </summary>
    enum CollisionTarget : int
    {
        First = 0,
        Second = 1
    }
}
