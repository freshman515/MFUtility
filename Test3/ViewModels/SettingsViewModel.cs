using Caliburn.Micro;

namespace Test3.ViewModels;

public class SettingsViewModel : Screen
{
    private readonly IEventAggregator _events;

    public SettingsViewModel(IEventAggregator events)
    {
        _events = events;
        DisplayName = "设置";
    }

    public void Notify()
    {
        _events.PublishOnUIThreadAsync("来自 Settings 的通知");
    }
}