using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkflowCore.AspNetCore.Json;

/// <summary>
/// Provides pre-configured JSON serializer options for WorkflowCore models.
/// </summary>
public static class WorkflowJsonSerializerOptions
{
    /// <summary>
    /// Gets JsonSerializerOptions configured to serialize WorkflowStatus and PointerStatus as strings.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = CreateDefaultOptions();

    /// <summary>
    /// Gets JsonSerializerOptions with indented formatting for readable output.
    /// </summary>
    public static JsonSerializerOptions Indented { get; } = CreateIndentedOptions();

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        options.Converters.Add(new WorkflowStatusJsonConverter());
        options.Converters.Add(new PointerStatusJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    private static JsonSerializerOptions CreateIndentedOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        options.Converters.Add(new WorkflowStatusJsonConverter());
        options.Converters.Add(new PointerStatusJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    /// <summary>
    /// Creates a new instance of JsonSerializerOptions with WorkflowCore converters applied.
    /// </summary>
    /// <param name="configure">Optional action to configure additional settings.</param>
    /// <returns>A configured JsonSerializerOptions instance.</returns>
    public static JsonSerializerOptions Create(Action<JsonSerializerOptions>? configure = null)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        options.Converters.Add(new WorkflowStatusJsonConverter());
        options.Converters.Add(new PointerStatusJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter());

        configure?.Invoke(options);

        return options;
    }
}
