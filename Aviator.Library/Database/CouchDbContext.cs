using Aviator.Library.Database.Config;
using Aviator.Library.Database.Types;
using CouchDB.Driver;
using CouchDB.Driver.Options;

namespace Aviator.Library.Database;


public class CouchDbContext : CouchContext
{
    public CouchDatabase<BasicAcarsCouch> BasicAcars { get; set; }

    public CouchDbContext(CouchOptions<CouchDbContext> options) : base(options) {}
}