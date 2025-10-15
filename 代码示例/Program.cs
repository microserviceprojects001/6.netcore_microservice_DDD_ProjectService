using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace 代码示例
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Program.cs 或 Startup.cs
            services.AddInfrastructure();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Application.Services.OrderService).Assembly);
            });
        }
    }
}