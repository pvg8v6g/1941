using System.Windows;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using UX.ViewModels.MainWindowViewModel;
using UX.Views.MainWindowView;

namespace UX.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    #region Fields

    private static ServiceProvider? ServiceProvider { get; set; }

    #endregion

    #region Constructor

    public App()
    {
        AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);

        var services = new ServiceCollection();

        // #region Register Popups
        //
        // services.AddSingleton<ProgressPopup>();
        //
        // #endregion

        #region Register View Models

        services.AddSingleton<MainWindowViewModel>();

        #endregion

        #region Assign View Models

        services.AddSingleton<MainWindowView>(provider => new MainWindowView { DataContext = provider.GetRequiredService<MainWindowViewModel>() });

        #endregion

        // #region Register Services
        //
        // services.AddSingleton<Func<Type, BaseViewModel>>(provider => viewModelType => (BaseViewModel) provider.GetRequiredService(viewModelType));
        // services.AddSingleton<Func<Type, EngineTask>>(provider => taskType => (EngineTask) provider.GetRequiredService(taskType));
        // services.AddSingleton<IJsonService, JsonService>();
        // services.AddSingleton<IDataService, DataService>();
        // services.AddSingleton<IConfigurationService, ConfigurationService>();
        // services.AddSingleton<INavigationService, NavigationService>();
        // services.AddSingleton<ILocationService, LocationService>();
        // services.AddSingleton<IGraphicsService, GraphicsService>();
        // services.AddSingleton<IIniDataService, IniDataService>();
        // services.AddSingleton<ObjectService<BaseModel>>();
        // services.AddSingleton<ArmorService<Armor>>();
        // services.AddSingleton<LocomotorService<Locomotor>>();
        // services.AddSingleton<ScienceService<Science>>();
        // services.AddSingleton<UpgradeService<Upgrade>>();
        // services.AddSingleton<WeaponService<Weapon>>();
        // services.AddSingleton<SpecialPowerService<SpecialPower>>();
        // services.AddSingleton<CommandSetService<CommandSet>>();
        // services.AddSingleton<CommandButtonService<CommandButton>>();
        // services.AddSingleton<ObjectCreationService<ObjectCreationList>>();
        //
        // #endregion
        //
        // #region Register Tasks
        //
        // services.AddSingleton<InitialDataLoadTask>();
        //
        // #endregion

        ServiceProvider = services.BuildServiceProvider();
    }

    #endregion

    #region Overrides

    protected override void OnStartup(StartupEventArgs e)
    {
        var startupForm = ServiceProvider?.GetRequiredService<MainWindowView>();
        startupForm?.Show();

        base.OnStartup(e);
    }

    #endregion

}
