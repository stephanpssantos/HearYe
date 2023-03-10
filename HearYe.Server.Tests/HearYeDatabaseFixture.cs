namespace HearYe.Server.Tests
{
    public class HearYeDatabaseFixture
    {
        //Server=(localdb)\\mssqllocaldb;Database=HearYe;Trusted_Connection=True;MultipleActiveResultSets=true
        private const string connectionString = "Data Source=(localdb)\\mssqllocaldb;Database=HearYe;Trusted_Connection=True;MultipleActiveResultsets=true;";
        private static readonly object _lock = new();
        private static bool _databaseInitialized;

        public HearYeDatabaseFixture()
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    using (var context = CreateContext())
                    {
                        DbInitializer.Initialize(context);
                        //context.Database.EnsureCreated();
                    }

                    _databaseInitialized = true;
                }
            }
        }

        public HearYeContext CreateContext()
        {
            DbContextOptionsBuilder<HearYeContext> options = new();
            options.UseSqlServer(connectionString);

            HearYeContext context = new(options.Options);

            return context;
        }
    }
}
