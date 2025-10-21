using System.IO;
using System.Threading;
using Frends.X12.ConvertToJson.Definitions;
using NUnit.Framework;

namespace Frends.X12.ConvertToJson.Tests;

[TestFixture]
public class UnitTests
{
    public const string X12Document = """
ISA*00*          *00*          *12*SENDERID       *12*RECEIVERID     *250101*1253*U*00401*000000001*0*T*>~
GS*PO*SENDERID*RECEIVERID*20250101*1253*1*X*004010~
ST*850*0001~
BEG*00*NE*PO123456**20250101~
REF*IA*123456~
PER*BD*John Buyer*TE*5551234567~
N1*BT*Buyer Company*92*12345~
N3*123 Buyer Street~
N4*Helsinki*Uusimaa*00100*FI~
N1*ST*Supplier Warehouse*92*98765~
N3*987 Supplier Road~
N4*Tampere*Pirkanmaa*33100*FI~
PO1*1*10*EA*15.00**BP*ABC123*VP*XYZ789~
PID*F****Widget Model X~
PO1*2*5*EA*9.50**BP*DEF456*VP*UVW567~
PID*F****Widget Model Y~
CTT*2~
SE*14*0001~
GE*1*1~
IEA*1*000000001~
""";

    [Test]
    public void ShouldRepeatContentWithDelimiter()
    {
        var input = new Input { InputX12 = X12Document };
        var options = new Options { ThrowErrorOnFailure = true, ErrorMessageOnFailure = null };

        var result = X12.ConvertToJson(input, options, CancellationToken.None);
        File.WriteAllText("result.json", result.Json);
        Assert.That(result.Json, Is.EqualTo("foobar, foobar, foobar"));
    }
}