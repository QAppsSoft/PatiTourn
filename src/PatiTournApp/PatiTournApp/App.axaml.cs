using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Common;
using Domain;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatiTournApp.Infrastructure;
using PatiTournApp.Views;
using ViewModels;
using ViewModels.Interfaces;

namespace PatiTournApp
{
    public partial class App : Application
    {
        public static IServiceProvider Services = null!; // Initialized before use

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ConfigureServiceProvider();

            ConfigureDatabase();

            var viewModel = Services.GetService<MainWindowViewModel>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow { DataContext = viewModel };

#if DEBUG
                var onShutdown = Observable.FromEventPattern<ShutdownRequestedEventArgs>(
                        handler => desktop.ShutdownRequested += handler,
                        handler => desktop.ShutdownRequested -= handler)
                    .Subscribe(_ => Services.GetService<PatiTournDataBaseContext>()?.Database.EnsureDeleted());
#endif
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainWindow { DataContext = viewModel };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void ConfigureDatabase()
        {
#if DEBUG
            using var context = Services.GetService<PatiTournDataBaseContext>();
            context?.Database.EnsureCreated();
#endif
#if !DEBUG
            using var context = Services.GetService<PatiTournDataBaseContext>();
            context?.Database.Migrate();
#endif
        }

        private static void ConfigureServiceProvider()
        {
            var services = ConfigureServices();
            Services = services.BuildServiceProvider();
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ISchedulerProvider, SchedulerProvider>();

            services.AddDbContext<PatiTournDataBaseContext>(options =>
                options.UseSqlite(@"Data Source=PatiTourn.db;"),
                ServiceLifetime.Transient,
                ServiceLifetime.Singleton);

            RegisterEntities(services);
            
            // Auto-register all view-models
            services.Scan(scan =>
            {
                scan.FromAssemblyOf<IViewModel>()
                    .AddClasses(classes => classes.AssignableTo<IViewModel>())
                    .AsSelfWithInterfaces()
                    .WithSingletonLifetime();
            });

            return services;
        }

        private static void RegisterEntities(IServiceCollection services)
        {
            services.Scan(scan =>
            {
                scan.FromAssemblyOf<IService>()
                    .AddClasses(classes => classes.AssignableTo(typeof(IEntityService<>)))
                    .AsMatchingInterface()
                    .WithTransientLifetime();
            });
        }
    }
}
