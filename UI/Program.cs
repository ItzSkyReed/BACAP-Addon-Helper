using BacapGenerator.Factories;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services;
using Core.DataComponents;
using Core.Registries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UI.Actions.Advancements;
using UI.Actions.Advancements.Debug;
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
    .ConfigureServices((context, services) =>
    {
        ComponentRegistry.RegisterAll();

        services.Configure<Dictionary<string, DatapackSettings>>(
            context.Configuration.GetSection("Datapacks"));

        var registryPath = context.Configuration.GetValue<string>("RegistryBasePath");

        services.AddSingleton<McRegistryLoader>(_ => new McRegistryLoader(registryPath!));
        services.AddSingleton<DatapackRegistry>();
        services.AddSingleton<MinecraftData>();

        services.AddTransient<IDatapackFactory, DatapackFactory>();
        services.AddTransient<DatapackLoaderService>();

        services.AddTransient<MainMenuAction>();
        services.AddTransient<IMainMenuAction, ManageAdvancementsMenu>();
        services.AddTransient<IManageAdvancementsAction, AdvancementStatsAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementInfoAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementDeleteAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementsFormatAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementsRecoverAction>();
        services.AddTransient<IManageAdvancementsAction, DebugAdvancementsMenuAction>();
        services.AddTransient<IManageAdvancementsAction, GenerateMilestonesAction>();
        services.AddTransient<IManageAdvancementsAction, GenerateDatapackFunctionsAction>();


        services.AddTransient<IDebugAdvancementsAction, ShowTechnicalInvalidAction>();
        services.AddTransient<IDebugAdvancementsAction, ShowBacapAdvancementsByTierAction>();

    })
    .Build();

// Resolve the main service and run the application
var loader = host.Services.GetRequiredService<DatapackLoaderService>();
loader.LoadAll();

var mainMenu = host.Services.GetRequiredService<MainMenuAction>();
await mainMenu.ExecuteAsync();