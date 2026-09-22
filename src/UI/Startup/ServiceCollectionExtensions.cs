using BacapGenerator.Configuration;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Datapacks.Services;
using Core.DataComponents;
using Core.Registries;
using Core.Registries.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UI.Actions.Advancements;
using UI.Actions.Advancements.Debug;
using UI.Actions.Datapacks;
using UI.Interfaces;
using UI.Menus;
using UI.Services;

namespace UI.Startup;

/// <summary>
/// Provides extension methods for modular dependency injection setup.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="services">The dependency injection service collection.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Binds and validates global and datapack configurations from application settings.
        /// </summary>
        /// <param name="configuration">The root configuration provider.</param>
        /// <returns>The service collection instance for method chaining.</returns>
        public IServiceCollection AddApplicationConfiguration(IConfiguration configuration)
        {
            services.AddOptions<GlobalConfig>()
                .Bind(configuration)
                .PostConfigure(options => options.Validate())
                .ValidateOnStart();

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<GlobalConfig>>().Value);

            services.Configure<Dictionary<string, DatapackSettings>>(
                configuration.GetSection("Datapacks"));

            return services;
        }

        /// <summary>
        /// Eagerly registers and loads Minecraft registries into the DI container.
        /// </summary>
        /// <param name="configuration">The root configuration provider.</param>
        /// <returns>The service collection instance for method chaining.</returns>
        /// <exception cref="RegistryLoadException">Thrown when registry path is invalid or files cannot be parsed.</exception>
        public IServiceCollection AddMinecraftRegistries(IConfiguration configuration)
        {
            ComponentRegistry.RegisterAll();

            var registryPath = configuration.GetValue<string>("registry_base_path");
            if (string.IsNullOrWhiteSpace(registryPath))
            {
                throw new RegistryLoadException(
                    "Configuration key 'registry_base_path' is missing or empty in configuration.",
                    RegistryErrorKind.DirectoryNotFound);
            }

            // McRegistryLoader throws RegistryLoadException on missing files or invalid JSON
            var loader = new McRegistryLoader(registryPath);
            var minecraftData = new MinecraftData(loader);

            services.AddSingleton(loader);
            services.AddSingleton(minecraftData);

            return services;
        }

        /// <summary>
        /// Registers core domain services responsible for datapack loading and registry management.
        /// </summary>
        /// <returns>The service collection instance for method chaining.</returns>
        public IServiceCollection AddDatapackServices()
        {
            services.AddSingleton<DatapackRegistry>();
            services.AddTransient<IDatapackFactory, DatapackFactory>();
            services.AddTransient<DatapackLoaderService>();
            services.AddTransient<ValidationRunnerService>();

            return services;
        }

        /// <summary>
        /// Registers all interactive TUI actions, menus, and submenus.
        /// </summary>
        /// <returns>The service collection instance for method chaining.</returns>
        [UsedImplicitly]
        public IServiceCollection AddUiActions()
        {
            // Root and Sub-Menus
            services.AddTransient<MainMenuAction>();
            services.AddTransient<IMainMenuAction, ManageAdvancementsMenu>();
            services.AddTransient<IMainMenuAction, ManageDatapacksMenu>();
            services.AddTransient<IMainMenuAction, ReleaseMenu>();

            // Advancement Management Actions
            services.AddTransient<IManageAdvancementsAction, AdvancementStatsAction>();
            services.AddTransient<IManageAdvancementsAction, AdvancementInfoAction>();
            services.AddTransient<IManageAdvancementsAction, AdvancementEditorAction>();
            services.AddTransient<IManageAdvancementsAction, AdvancementFunctionsSetupAction>();
            services.AddTransient<IManageAdvancementsAction, AdvancementsFormatAction>();
            services.AddTransient<IManageAdvancementsAction, AdvancementsRecoverAction>();
            services.AddTransient<IManageAdvancementsAction, GenerateMilestonesAction>();
            services.AddTransient<IManageAdvancementsAction, DebugAdvancementsMenuAction>();

            // Advancement Debug Actions
            services.AddTransient<IDebugAdvancementsAction, ShowTechnicalInvalidAction>();
            services.AddTransient<IDebugAdvancementsAction, ShowBacapAdvancementsByTierAction>();

            // Datapack Actions
            services.AddTransient<IManageDatapacksAction, GenerateDatapackFunctionsAction>();
            services.AddTransient<IManageDatapacksAction, ValidateDatapacksAction>();
            services.AddTransient<IManageDatapacksAction, SyncWithTranslationPack>();

            return services;
        }
    }
}