namespace PortaJel_Blazor.Classes;

/// <summary>
/// Represents a property for connectors within the PortaJel application.
/// </summary>
public class ConnectorProperty(string label, string description, object value, bool protectValue)
{
    /// <summary>
    /// Gets or sets the label of the connector property.
    /// </summary>
    public string Label { get; private set; } = label;

    /// <summary>
    /// Gets or sets the description of the connector property.
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// Gets or sets the value associated with this property.
    /// </summary>
    public object Value { get; set; } = value;

    /// <summary>
    /// Gets or sets a value indicating whether the property value is protected.
    /// When set to true, the value should be hidden.
    /// </summary>
    public bool ProtectValue { get; private set; } = protectValue;
}