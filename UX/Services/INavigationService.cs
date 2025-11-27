using UX.ViewModels.BaseViewModel;

namespace UX.Services;

public interface INavigationService
{

    BaseViewModel ActivePage { get; set; }

    BaseViewModel TopBar { get; set; }

    void SetTopBar<T>() where T : BaseViewModel;

    void NavigateTo<T>() where T : BaseViewModel;

}
