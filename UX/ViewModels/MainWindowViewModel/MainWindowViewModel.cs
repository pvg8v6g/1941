using UX.Services;

namespace UX.ViewModels.MainWindowViewModel;

public class MainWindowViewModel(INavigationService navigationService) : BaseViewModel.BaseViewModel
{

    #region Properties

    public INavigationService NavigationService => navigationService;

    #endregion

    #region Actions

    protected override void LoadedAction()
    {
        NavigationService.NavigateTo<GameViewModel.GameViewModel>();
    }

    #endregion

}
