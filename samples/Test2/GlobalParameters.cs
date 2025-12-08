using System.Collections.ObjectModel;
using System.ComponentModel;
using MFUtility.WPF.Bases;

namespace Test2;

public class GlobalParameters : INotifyPropertyChanged
{
    private static readonly Lazy<GlobalParameters> _lazy =
        new Lazy<GlobalParameters>(() => new GlobalParameters());
    public static GlobalParameters Instance => _lazy.Value;

    private ObservableCollection<string> _logs;
    public ObservableCollection<string> Logs
    {
        get => _logs;
        set
        {
            _logs = value;
            OnPropertyChanged(nameof(Logs));
        }
    }

    private GlobalParameters()
    {
        Logs = new ObservableCollection<string>();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}