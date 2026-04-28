using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EleonHotel.Data;

namespace EleonHotel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Загружаем конфигурацию
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Настраиваем DI контейнер
            var services = new ServiceCollection();
            services.AddDbContext<HotelDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            ServiceProvider = services.BuildServiceProvider();

            // Создаём БД, если её нет
            using (var context = ServiceProvider.GetRequiredService<HotelDbContext>())
            {
                context.Database.Migrate();
            }
        }
    }
}
