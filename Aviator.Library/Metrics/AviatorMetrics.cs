using Aviator.Library.Acars;

namespace Aviator.Library.Metrics;

public class AviatorMetrics(List<IAviatorMetrics> metricsList) : IAviatorMetrics
{
    public void IncreaseMessagesTotal(BasicAcars basicAcars)
    {
        foreach (var aviatorMetrics in metricsList)
        {
            aviatorMetrics.IncreaseMessagesTotal(basicAcars);
        }
    }

    public void AddSignalLevel(BasicAcars? basicAcars, double? level)
    {
        if (basicAcars is null || level is null)
        {
            return;
        }
        foreach (var aviatorMetrics in metricsList)
        {
            aviatorMetrics.AddSignalLevel(basicAcars, (double)level);
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

    public void SetOutputStatus(string endpoint, bool connected)
    {
        foreach (var aviatorMetrics in metricsList)
        {
            aviatorMetrics.SetOutputStatus(endpoint, connected);
        }
    }
}