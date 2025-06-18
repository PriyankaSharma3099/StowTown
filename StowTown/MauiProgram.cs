using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using StowTown.HelperService;
using StowTown.Models;
using StowTown.Pages.RadioStations;
using StowTown.Pages.Reports;
using StowTown.Services;
using System;

namespace StowTown
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

           // Add support for appsettings.json
           var config = new ConfigurationBuilder()
               .SetBasePath(FileSystem.AppDataDirectory) // workaround for MAUI path
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .Build();

           // Save config to DI container
           builder.Configuration.AddConfiguration(config);

            // Register DbContext and inject config
            builder.Services.AddDbContext<StowTownDbContext>(options =>
            {
                var connectionString = config.GetConnectionString("StowTownConnectionString");
                options.UseSqlServer(connectionString);
            });

            //        var config = new ConfigurationBuilder()
            //.SetBasePath(FileSystem.AppDataDirectory)
            //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //.Build();

            //        var connectionString = config.GetConnectionString("StowTownConnectionString");

            //        var optionsBuilder = new DbContextOptionsBuilder<StowTownDbContext>();
            //        optionsBuilder.UseSqlServer(connectionString);

            //        var context = new StowTownDbContext(optionsBuilder.Options);
            //        builder.Services.AddDbContext<StowTownDbContext>(options =>
            //        {
            //            var config = new ConfigurationBuilder()
            //                .SetBasePath(FileSystem.AppDataDirectory)
            //                .AddJsonFile("appsettings.json", optional: true)
            //                .Build();

            //            var connectionString = config.GetConnectionString("StowTownConnectionString");
            //            options.UseSqlServer(connectionString);
            //        });
            //        builder.Services.AddSingleton(context);


            builder
                .UseMauiApp<App>()
                  .UseLiveCharts()     //Register LiveCharts
                    .UseSkiaSharp()      //Required for rendering charts
                    .UseMauiCommunityToolkit()

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
           // builder.Services.AddTransient<HomeDashboard>();
            builder.Services.AddTransient<RadioStationService>();
            builder.Services.AddTransient<CreateRadioStation>();
            builder.Services.AddTransient<Graph>();
            builder.Services.AddSingleton<EmailService>();
            

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
