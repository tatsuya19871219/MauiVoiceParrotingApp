using CommunityToolkit.Mvvm.ComponentModel;
using MauiVoiceParrotingApp.Services;

namespace MauiVoiceParrotingApp.ViewModels;

public partial class VoiceParrotingServiceStateViewModel : ObservableObject
{
    [ObservableProperty] bool isRecBusy;
    [ObservableProperty] bool isPlayBusy;

    [ObservableProperty] double recPosition;
    [ObservableProperty] double playPosition;

    [ObservableProperty] bool isAvailable;
    [ObservableProperty] bool isRunning;

    readonly VoiceParrotingService _service;

    public VoiceParrotingServiceStateViewModel(VoiceParrotingService service)
    {
        _service = service;

        IsRecBusy = IsPlayBusy = false;
        recPosition = playPosition = 0;

        IsAvailable = true;
        IsRunning = false;

        RunStateTracker();
    }

    async void RunStateTracker()
    {
        while(true)
        {
            RecPosition = _service.GetRecorderProgress();
            PlayPosition = _service.GetTrackerProgress();

            IsRecBusy = _service.IsRecorderRunning;
            IsPlayBusy = _service.IsTrackerRunning;

            IsRunning = _service.IsRunning;
            IsAvailable = !_service.IsRunning;

            await Task.Delay(100);
        }
    }

}
