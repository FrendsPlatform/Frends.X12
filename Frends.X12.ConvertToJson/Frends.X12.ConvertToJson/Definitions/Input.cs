using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.X12.ConvertToJson.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Input X12 document.
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
    [DefaultValue("")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Edi { get; set; } = string.Empty;
}