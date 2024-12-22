using Aviator.Acars.Entities;

namespace Aviator.Acars.Metrics;

public class AcarsMetrics(ICollection<IAcarsMetrics> metricsList) : IAcarsMetrics
{
    public async Task IncreaseAsync(AcarsType type, BasicAcars acars, CancellationToken cancellationToken = default)
    {
        foreach (var metrics in metricsList) await metrics.IncreaseAsync(type, acars, cancellationToken);
    }
}