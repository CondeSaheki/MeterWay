namespace MeterWay.Overlay;

/// <summary>
/// Represents the essential information for any overlay. This is mandatory and must be included in your overlay.
/// </summary>
public class BasicOverlayInfo
{
    /// <summary>
    /// Gets or initializes the name of the overlay. This is the name that will be visible to users, so choose wisely.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets or initializes the name of the overlay author.
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// Gets or initializes the description of the overlay, detailing its behavior, appearance, and other relevant information.
    /// </summary>
    public string? Description { get; init; }

    /*
        Additional examples and information will be added here in future updates.
    */
}