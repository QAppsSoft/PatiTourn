using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Common;
using DataModel;
using Domain;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatiTournApp.Infrastructure;
using PatiTournApp.Views;
using ViewModels;
using ViewModels.Interfaces;
using ViewModels.Modules.Skaters;
using ViewModels.Modules.Teams;

namespace PatiTournApp
{
    public partial class App : Application
    {
        private static IServiceProvider services = null!; // Initialized before use

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ConfigureServiceProvider();

            ConfigureDatabase();

            var viewModel = services.GetService<MainWindowViewModel>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow { DataContext = viewModel };

#if DEBUG
                var onShutdown = Observable.FromEventPattern<ShutdownRequestedEventArgs>(
                        handler => desktop.ShutdownRequested += handler,
                        handler => desktop.ShutdownRequested -= handler)
                    .Subscribe(_ => services.GetService<PatiTournDataBaseContext>()?.Database.EnsureDeleted());
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
            using var context = services.GetService<PatiTournDataBaseContext>();
            context?.Database.EnsureCreated();
#endif
#if !DEBUG
            using var context = Services.GetService<PatiTournDataBaseContext>();
            context?.Database.Migrate();
#endif
        }

        private static void ConfigureServiceProvider()
        {
            App.services = ConfigureServices().BuildServiceProvider();
        }

        private static IServiceCollection ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ISchedulerProvider, SchedulerProvider>();

            serviceCollection.AddDbContext<PatiTournDataBaseContext>(options =>
                {
                    options.UseSqlite(@"Data Source=PatiTourn.db;")
                        .UseLazyLoadingProxies();
                },
                ServiceLifetime.Transient,
                ServiceLifetime.Singleton);

            RegisterEntities(serviceCollection);

            RegisterViewModels(serviceCollection);

            RegisterParameterizedViewModels(serviceCollection);

            return serviceCollection;
        }

        private static void RegisterParameterizedViewModels(IServiceCollection serviceCollection)
        {
            // Register view-models factories with dynamic parameter
            serviceCollection.AddTransient(provider => new Func<TeamsViewModel, Competition, SkatersViewModel>(
                (teamsViewModel, competition) => ActivatorUtilities.CreateInstance<SkatersViewModel>(provider,
                    teamsViewModel,
                    competition)));

            serviceCollection.AddTransient(provider => new Func<Competition, TeamsViewModel>(competition =>
                ActivatorUtilities.CreateInstance<TeamsViewModel>(provider, competition)));
        }

        private static void RegisterViewModels(IServiceCollection serviceCollection)
        {
            // Auto-register all view-models
            serviceCollection.Scan(scan =>
            {
                scan.FromAssemblyOf<IViewModel>()
                    .AddClasses(classes => classes.AssignableTo<IViewModel>())
                    .AsSelfWithInterfaces()
                    .WithSingletonLifetime();
            });
        }

        private static void RegisterEntities(IServiceCollection serviceCollection)
        {
            serviceCollection.Scan(scan =>
            {
                scan.FromAssemblyOf<IService>()
                    .AddClasses(classes => classes.AssignableTo(typeof(IEntityService<>)))
                    .AsMatchingInterface()
                    .WithTransientLifetime();
            });
        }
    }
}
