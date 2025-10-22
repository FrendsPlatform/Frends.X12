namespace Frends.X12.ConvertToJson.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Result of converting Edifact to JSON.
    /// </summary>
    /// <example>
    /// {
    ///     "Edifact": {
    ///         "UNB": { ... }
    ///         "TSINVOIC": { ... }
    ///         "UNZ": { ... }
    ///     }
    /// }
    /// </example>
    public string Json { get; set; } = string.Empty;

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}