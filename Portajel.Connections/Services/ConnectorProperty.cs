namespace Portajel.Connections.Services;

/// <summary>
/// Represents a property for connectors within the PortaJel application.
/// </summary>
public class ConnectorProperty
{
    /// <summary>
    /// Parameterless constructor for JSON deserialization
    /// </summary>
    public ConnectorProperty()
    {
    }

    /// <summary>
    /// Full constructor
    /// </summary>
    public ConnectorProperty(string label, string description, object value, bool protectValue, bool userVisible)
    {
        Label = label;
        Description = description;
        Value = value;
        ProtectValue = protectValue;
        UserVisisble = userVisible;
    }

    /// <summary>
    /// Gets or sets the label of the connector property.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the connector property.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value associated with this property.
    /// </summary>
    public object Value { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the property value is protected.
    /// When set to true, the value should be hidden.
    /// </summary>
    public bool ProtectValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property value is allowed to be shown to the user.
    /// When set to true, the value can be shown to the user in a UI.
    /// </summary>
    public bool UserVisisble { get; set; }
}
