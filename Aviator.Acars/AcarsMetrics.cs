using Aviator.Acars.Entities;
using Aviator.Acars.Metrics;

namespace Aviator.Acars;

public class AcarsMetrics(List<IAcarsMetrics> metricsList) : IAcarsMetrics
{
    public async Task Increase(AcarsType type, BasicAcars acars, CancellationToken cancellationToken = default)
    {
        foreach (var metrics in metricsList)
        {
            await metrics.Increase(type, acars, cancellationToken);
        }
    }
}