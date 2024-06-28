using Aviator.Library.Acars;

namespace Aviator.Library.Metrics;

public interface IAviatorMetrics
{
    public void IncreaseMessagesTotal(BasicAcars basicAcars);
    public void AddSignalLevel(BasicAcars basicAcars, double? level);
    public void AddNoiseLevel(BasicAcars basicAcars, double? level);
    public void SetOutputStatus(string endpoint, bool connected);
}