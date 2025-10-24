namespace Frends.X12.CreateFromJson.Definitions;

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
    /// Edi string created from Json.
    /// </summary>
    /// <example>
    /// ISA*00*          *00*          *12*SENDERID       *12*RECEIVERID     *250101*1253*U*00401*000000001*0*T*>~
    /// GS*PO*SENDERID*RECEIVERID*20250101*1253*1*X*004010~
    ///     ST*850*0001~
    /// BEG*00*NE*PO123456**20250101~
    /// REF*IA*123456~
    /// PER*BD*John Buyer*TE*5551234567~
    /// ...
    /// SE*14*0001~
    /// GE*1*1~
    /// IEA*1*000000001~
    /// </example>
    public string Edi { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}