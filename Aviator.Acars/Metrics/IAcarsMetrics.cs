using Aviator.Acars.Entities;

namespace Aviator.Acars.Metrics;

public interface IAcarsMetrics
{
    Task IncreaseAsync(AcarsType type, BasicAcars acars, CancellationToken cancellationToken = default);
}