using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAndMigrateTenantDatabases(this IServiceCollection services, IConfiguration config)
        {
            //Получаем настройки по умолчанию из файла appsettings.json
            var options = services.GetOptions<TenantSettings>(nameof(TenantSettings));
            var defaultConnectionString = options.Defaults?.ConnectionString;
            var defaultDbProvider = options.Defaults?.DBProvider;
            //
            if (defaultDbProvider.ToLower() == "mssql")
            {
                //Регистрирует ApplicationDbContext с помощью пакета SQLServer
                services.AddDbContext<ApplicationDbContext>(m => m.UseSqlServer(e => e.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
            }
            var tenants = options.Tenants;

            //Перебирает список настроенных арендаторов. Если у арендатора не объявлена строка подключения,
            //ему присваивается строка подключения по умолчанию.
            //Отсюда мы извлекаем службу DBContext, устанавливаем ее подключение к строке подключения арендатора и,
            //наконец, выполняем миграцию
            foreach (var tenant in tenants)
            {
                string connectionString;
                if (string.IsNullOrEmpty(tenant.ConnectionString))
                {
                    connectionString = defaultConnectionString;
                }
                else
                {
                    connectionString = tenant.ConnectionString;
                }
                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.SetConnectionString(connectionString);
                if (dbContext.Database.GetMigrations().Count() > 0)
                {
                    dbContext.Database.Migrate();
                }
            }
            return services;
        }

        //общий метод для получения конфигурации из AppSettings.json в статический файл
        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(sectionName);
            var options = new T();
            section.Bind(options);
            return options;
        }
    }
}
