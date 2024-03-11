using System.Net;
using Aviator.Library.IO.Output;
using EndPoint = Aviator.Library.IO.EndPoint;

namespace Aviator.Library.Acars;

public class AcarsOutputState
{
    public EndPoint? EndPoint { get; init; }
    public string State { get; init; } = string.Empty;
}