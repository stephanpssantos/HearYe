using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public static class HearYeContextExtensions
    {
        public static IServiceCollection AddHearYeContext(
            this IServiceCollection services, string connectionString =
            "Data Source=.;Initial Catalog=HearYe;"
            + "Integrated Security=true;MultipleActiveResultsets=true;") 
        {
            services.AddDbContext<HearYeContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
