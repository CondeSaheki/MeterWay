namespace MeterWay.Overlay;

/// <summary>
/// Interface representing the configuration for an overlay.
/// </summary>
public interface IBasicOverlayConfiguration
{
    /// <summary>
    /// Gets or sets the version of the configuration.
    /// This property is used to keep track of the configuration schema version, allowing for proper handling of updates and migrations.
    /// </summary>
    int Version { get; set; }
}