using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.X12.CreateFromJson.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// X12 in JSON format. The JSON format should be the same
    /// as produced by Frends when converting X12 documents to
    /// JSON.
    /// </summary>
    /// <example>
    /// {
    ///     "X12": {
    ///         ...
    ///     }
    /// }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string Json { get; set; } = string.Empty;
}