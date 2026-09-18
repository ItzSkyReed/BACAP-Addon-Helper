using System.Diagnostics;
using System.Text;
using BacapGenerator.Configuration;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Datapacks.Services;
using Core.DataComponents;
using Core.Registries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NReco.Logging.File;
using UI.Actions.Advancements;
using UI.Actions.Advancements.Debug;
using UI.Actions.Datapacks;
using UI.Configuration;
using UI.Interfaces;
using UI.Menus;
using UI.Services;

const string configFileName = "config.yaml";

const string embeddedConfigResourceName = "UI.default_config.yaml";

// Check config and fail-fast if it was missing
YamlConfigBootstrapper.EnsureConfigExists(configFileName, embeddedConfigResourceName);

Console.OutputEncoding = Encoding.UTF8;

// Build the host with DI and Configuration
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.Sources.Clear();
        config.AddYamlFile(configFileName, optional: false, reloadOnChange: true);
        config.ExpandTemplates();
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
        // Bind root configuration directly to GeneratorOptions
        services.AddOptions<GlobalConfig>()
            .Bind(context.Configuration)
            .PostConfigure(options => options.Validate())
            .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<GlobalConfig>>().Value);

        ComponentRegistry.RegisterAll();

        services.Configure<Dictionary<string, DatapackSettings>>(
            context.Configuration.GetSection("Datapacks"));

        var registryPath = context.Configuration.GetValue<string>("registry_base_path");

        services.AddSingleton<McRegistryLoader>(_ => new McRegistryLoader(registryPath!));
        services.AddSingleton<DatapackRegistry>();
        services.AddSingleton<MinecraftData>();

        services.AddTransient<IDatapackFactory, DatapackFactory>();
        services.AddTransient<DatapackLoaderService>();

        // UI menus
        services.AddTransient<MainMenuAction>();
        services.AddTransient<IMainMenuAction, ManageAdvancementsMenu>();
        services.AddTransient<IMainMenuAction, ManageDatapacksMenu>();
        services.AddTransient<IMainMenuAction, ReleaseMenu>();
        services.AddTransient<ValidationRunnerService>();


        // Advancement actions
        services.AddTransient<IManageAdvancementsAction, AdvancementStatsAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementInfoAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementEditorAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementFunctionsSetupAction>();

        services.AddTransient<IManageAdvancementsAction, AdvancementsFormatAction>();
        services.AddTransient<IManageAdvancementsAction, AdvancementsRecoverAction>();

        services.AddTransient<IManageAdvancementsAction, GenerateMilestonesAction>();
        services.AddTransient<IManageAdvancementsAction, DebugAdvancementsMenuAction>();

        // Advancement debug actions
        services.AddTransient<IDebugAdvancementsAction, ShowTechnicalInvalidAction>();
        services.AddTransient<IDebugAdvancementsAction, ShowBacapAdvancementsByTierAction>();


        // Datapack actions
        services.AddTransient<IManageDatapacksAction, GenerateDatapackFunctionsAction>();
        services.AddTransient<IManageDatapacksAction, ValidateDatapacksAction>();
        services.AddTransient<IManageDatapacksAction, SyncWithTranslationPack>();

    })
    .Build();

// Resolve the main service and run the application
var loader = host.Services.GetRequiredService<DatapackLoaderService>();
loader.LoadAll();

var mainMenu = host.Services.GetRequiredService<MainMenuAction>();
await mainMenu.ExecuteAsync();