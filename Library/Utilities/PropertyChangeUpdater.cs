using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Library.Utilities;

public class PropertyChangeUpdater : INotifyPropertyChanged
{

    #region Functions

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion

}
