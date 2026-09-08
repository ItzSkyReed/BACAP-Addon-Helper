using System.Diagnostics;
using BacapGenerator.Factories;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services;
using Core.DataComponents;
using Core.Registries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using UI.Actions.Advancements;
using UI.Actions.Advancements.Debug;
using UI.Actions.Datapacks;
using UI.Configuration;
using UI.Interfaces;
using UI.Menus;

const string configFileName = "config.yaml";

const string embeddedConfigResourceName = "UI.default_config.yaml";

// Check config and fail-fast if it was missing
YamlConfigBootstrapper.EnsureConfigExists(configFileName, embeddedConfigResourceName);

// Build the host with DI and Configuration
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.Sources.Clear();
        config.AddYamlFile(configFileName, optional: false, reloadOnChange: true);
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();

        // Registers provider at runtime only when a debugger is actively attached
        if (Debugger.IsAttached)
            logging.AddDebug();

        var filePath = context.Configuration.GetValue<string>("logging:file_path");
        if (!string.IsNullOrWhiteSpace(filePath))
            logging.AddFile(filePath);

        logging.AddConfiguration(context.Configuration.GetSection("logging"));
    })
    .ConfigureServices((context, services) =>
    {
        ComponentRegistry.RegisterAll();

        services.Configure<Dictionary<string, DatapackSettings>>(
            context.Configuration.GetSection("Datapacks"));

        var registryPath = context.Configuration.GetValue<string>("registry_base_path");

        services.AddSingleton<McRegistryLoader>(_ => new McRegistryLoader(registryPath!));
        services.AddSingleton<DatapackRegistry>();
        services.AddSingleton<MinecraftData>();

        services.AddTransient<IDatapackFactory, DatapackFactory>();
        services.AddTransient<DatapackLoaderService>();

        services.AddTransient<MainMenuAction>();
        services.AddTransient<IMainMenuAction, ManageAdvancementsMenu>();

        services.AddTransient<IManageAdvancementsAction, AdvancementStatsAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementInfoAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementEditorAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementFunctionsSetupAction>();

        services.AddTransient<IManageAdvancementsAction, AdvancementsFormatAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementsRecoverAction>();

        services.AddTransient<IManageAdvancementsAction, GenerateMilestonesAction>();
        services.AddTransient<IManageAdvancementsAction, GenerateDatapackFunctionsAction>();

        services.AddTransient<IManageAdvancementsAction, DebugAdvancementsMenuAction>();



        services.AddTransient<IDebugAdvancementsAction, ShowTechnicalInvalidAction>();
        services.AddTransient<IDebugAdvancementsAction, ShowBacapAdvancementsByTierAction>();

    })
    .Build();

// Resolve the main service and run the application
var loader = host.Services.GetRequiredService<DatapackLoaderService>();
loader.LoadAll();

var mainMenu = host.Services.GetRequiredService<MainMenuAction>();
await mainMenu.ExecuteAsync();