using Aviator.Library.Acars;

namespace Aviator.Library.Metrics;

public class AviatorMetrics(List<IAviatorMetrics> metricsList) : IAviatorMetrics
{
    public void IncReceivedMessagesTotal(BasicAcars basicAcars)
    {
        foreach (var aviatorMetrics in metricsList)
        {
            aviatorMetrics.IncReceivedMessagesTotal(basicAcars);
        }
    }

    public void AddSigLevel(BasicAcars? basicAcars, double? level)
    {
        if (basicAcars is null || level is null)
        {
            return;
        }
        foreach (var aviatorMetrics in metricsList)
        {
            aviatorMetrics.AddSigLevel(basicAcars, (double)level);
        }
    }

    public void AddNoiseLevel(BasicAcars? basicAcars, double? level)
    {
        if (basicAcars is null || level is null)
        {
            return;
        }
        foreach (var aviatorMetrics in metricsList)
        {
            aviatorMetrics.AddNoiseLevel(basicAcars, (double)level);
        }
    }

    public void SetOutputStatus(string type, string endpoint, bool connected)
    {
        foreach (var aviatorMetrics in metricsList)
        {
            aviatorMetrics.SetOutputStatus(type, endpoint, connected);
        }
    }
}