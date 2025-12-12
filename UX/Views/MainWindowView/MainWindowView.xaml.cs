using System.Windows.Input;
using UX.Views.LabelsEditor;

namespace UX.Views.MainWindowView;

public partial class MainWindowView
{

    public MainWindowView()
    {
        InitializeComponent();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F12)
        {
            var win = new LabelsEditorWindow();
            win.Owner = this;
            win.Show();
        }
    }

}
