using System.Collections;

namespace Aviator.Acars.Database;

public class AcarsDatabase(ICollection<IAcarsDatabase> acarsDatabases) : IAcarsDatabase
{
    public async Task InsertAsync(byte[] bytes, CancellationToken cancellationToken = default)
    {
        foreach (var acarsDatabase in acarsDatabases)
        {
            await acarsDatabase.InsertAsync(bytes, cancellationToken).ConfigureAwait(false);
        }
    }
}