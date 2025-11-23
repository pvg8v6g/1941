using System.Windows.Input;

namespace Library.Utilities;

public class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{

    #region Properties

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    #endregion

    #region ICommand Members

    public bool CanExecute(object? parameter)
    {
        return canExecute == null || canExecute();
    }

    public void Execute(object? parameter)
    {
        execute();
    }

    #endregion

}

public class RelayCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null) : ICommand
{

    #region Properties

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    #endregion

    #region ICommand Members

    public bool CanExecute(object? parameter)
    {
        if (parameter is null)
        {
            return false;
        }

        return canExecute == null || canExecute((T) parameter);
    }

    public void Execute(object? parameter)
    {
        if (parameter is null)
        {
            return;
        }

        execute((T) parameter);
    }

    #endregion

}
