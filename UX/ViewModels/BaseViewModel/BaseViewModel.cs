using Library.Utilities;

namespace UX.ViewModels.BaseViewModel;

public class BaseViewModel : PropertyChangeUpdater
{

    #region Properties

    #endregion

    #region Actions

    public RelayCommand LoadedCommand => new(LoadedAction);

    protected virtual void LoadedAction()
    {
    }

    #endregion

}
