using Aviator.Library.Acars;

namespace Aviator.Library.Metrics;

public interface IAviatorMetrics
{
    public void IncReceivedMessagesTotal(BasicAcars basicAcars);
    public void AddSigLevel(BasicAcars basicAcars, double? level);
    public void AddNoiseLevel(BasicAcars basicAcars, double? level);
    public void SetOutputStatus(string type, string endpoint, bool connected);
}