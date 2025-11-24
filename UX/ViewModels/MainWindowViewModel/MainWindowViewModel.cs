namespace UX.ViewModels.MainWindowViewModel;

public class MainWindowViewModel : BaseViewModel.BaseViewModel
{
    public string WorldWaterImagePath => "/Graphics/Images/world_water.svg";
    public string WorldImagePath => "/Graphics/Images/world.svg";

    #region Actions

    protected override void LoadedAction()
    {
    }

    #endregion

}
