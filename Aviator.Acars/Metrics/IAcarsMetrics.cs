using Aviator.Acars.Entities;

namespace Aviator.Acars.Metrics;

public interface IAcarsMetrics
{
    Task Increase(AcarsType type, BasicAcars acars, CancellationToken cancellationToken = default);
}