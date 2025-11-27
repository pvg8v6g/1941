using Library.Utilities;
using UX.ViewModels.BaseViewModel;

namespace UX.Services;

public class NavigationService(Func<Type, BaseViewModel> viewModelFactory) : PropertyChangeUpdater, INavigationService
{

    #region Properties

    public BaseViewModel ActivePage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = null!;

    public BaseViewModel TopBar
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = null!;

    #endregion

    #region Functions

    public void SetTopBar<T>() where T : BaseViewModel
    {
        TopBar = viewModelFactory.Invoke(typeof(T));
    }

    public void NavigateTo<T>() where T : BaseViewModel
    {
        ActivePage = viewModelFactory.Invoke(typeof(T));
    }

    #endregion

}
