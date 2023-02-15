using MauiVoiceParrotingApp.Services;

namespace MauiVoiceParrotingApp;

public partial class MainPage : ContentPage
{
    bool _isAppBusy = false;
    public bool IsAppBusy
    {
        get { return _isAppBusy; }
        set 
        { 
            _isAppBusy = value;

            if(value) DisableUIs();
            else EnableUIs();
        }
    }

    private Color _busyColor;
    private Color _readyColor;

    //private double _recordingTime = 10;
    private double _delay = 0;

    VoiceParrotingService service = new();

    public MainPage()
    {
        InitializeComponent();

        //MyDictionary.TryGetValue("BusyColor", out object value);
        TrySetValue<Color>("BusyColor", out _busyColor);
        TrySetValue<Color>("ReadyColor", out _readyColor);

    }

    private bool TrySetValue<TypeOfValue>(string key, out TypeOfValue state)
    {
        bool result = MyDictionary.TryGetValue(key, out object value);

        state = (TypeOfValue)value;

        return result;
    }

    async private void StartButton_Clicked(object sender, EventArgs e)
    {
        // See: https://github.com/dotnet/maui/issues/7377#issuecomment-1311552365
        await Task.Yield();

        // Start audio recorder & tracker
        //var service = new VoiceParrotingService(DelayTimeSlider.Value);
        _delay = DelayTimeSlider.Value;
        service.SetDelayTime(_delay);

        RunProgressBar(VoiceParrotingService.s_recTime + _delay);

        IsAppBusy = true;
        //DisableUIs();
        //await Task.Delay(100);

        await service.Invoke();
        //EnableUIs();

        IsAppBusy = false;
    }

    async private void RecordButton_Clicked(object sender, EventArgs e)
    {
        await Task.Yield();

        RunProgressBar(VoiceParrotingService.s_recTime);

        IsAppBusy = true;
        //DisableUIs();
        //await Task.Delay(100);

        await service.RecorderInvoke();
        //EnableUIs();
        IsAppBusy = false;
    }

    async private void PlayButton_Clicked(object sender, EventArgs e)
    {
        await Task.Yield();

        RunProgressBar(VoiceParrotingService.s_recTime);

        IsAppBusy = true;
        //DisableUIs();
        //await Task.Delay(100);

        await service.TrackerInvoke();
        //EnableUIs();

        IsAppBusy = false;
    }

    async private void RunProgressBar(double timeInSec)
    {
        MyProgress.Progress = 0;
        uint milli = (uint)(timeInSec * 1000);
        MyProgress.ProgressTo(1, milli, Easing.Linear);
    }

    void DisableUIs()
    {
        RecordButton.IsEnabled = false;
        PlayButton.IsEnabled = false;
        StartButton.IsEnabled = false;

        //BusyIndicator.IsVisible = true;
        //BusyIndicator.BackgroundColor = Colors.Red;
        BusyIndicator.BackgroundColor = _busyColor;

        DelayTimeSlider.IsEnabled = false;
    }

    void EnableUIs()
    {
        RecordButton.IsEnabled = true;
        PlayButton.IsEnabled = true;
        StartButton.IsEnabled = true;

        //BusyIndicator.IsVisible = false;
        //BusyIndicator.BackgroundColor = Colors.LightGreen;
        BusyIndicator.BackgroundColor = _readyColor;

        DelayTimeSlider.IsEnabled = true;
    }
}