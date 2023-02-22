using MauiVoiceParrotingApp.Services;
using MauiVoiceParrotingApp.ViewModels;

namespace MauiVoiceParrotingApp;

public partial class MainPage : ContentPage
{
    //bool _isAppBusy = false;
    //public bool IsAppBusy
    //{
    //    get { return _isAppBusy; }
    //    set
    //    {
    //        _isAppBusy = value;

    //        if (value) DisableUIs();
    //        else EnableUIs();
    //    }
    //}

    // No longer used
    //private Color _busyColor;
    //private Color _readyColor;

    //private double _recordingTime = 10;
    //private double _delay = 0;

    VoiceParrotingService _service; // = new();

    public MainPage()
    {
        
        InitializeComponent();

        //MyDictionary.TryGetValue("BusyColor", out object value);
        //TrySetValue<Color>("BusyColor", out _busyColor);
        //TrySetValue<Color>("ReadyColor", out _readyColor);

        Initialize();

    }

    async void Initialize()
    {
        await QuitAppIfMicPermissionIsNotGranted();

        _service = new VoiceParrotingService(DelayTimeSlider.Value);

        var vm = new VoiceParrotingServiceStateViewModel(_service);

        BindingContext = vm;

#if WINDOWS

        //vm.UpdateDeviceList();
        RecorderPicker.ItemsSource = _service.RecorderList;
        PlayerPicker.ItemsSource = _service.PlayerList;

        RecorderPicker.SelectedIndexChanged += RecorderSelectedIndexChanged;
        PlayerPicker.SelectedIndexChanged += PlayerSelectedIndexChanged;

        //RecorderPicker.SelectedIndex = 0;
        //PlayerPicker.SelectedIndex = 0;
        RecorderPicker.SelectedIndex = _service.RecorderList.Count - 1;
        PlayerPicker.SelectedIndex = _service.PlayerList.Count - 1;
#endif

    }

#if WINDOWS
    async void RecorderSelectedIndexChanged(object sender, EventArgs e)
    {
        Picker recorderPicker = (Picker)sender;

        if (recorderPicker.SelectedIndex < 0) return;

        if (recorderPicker.Items.Count != _service.RecorderList.Count)
        {
            // ItemsSource should be updated
            await DisplayAlert("Notification", "Recorder list is updated. Please select again.", "OK");
            recorderPicker.ItemsSource = _service.RecorderList;
            return;
        }

        _service.ChangeRecorderDevice(recorderPicker.SelectedIndex);
    }

    async void PlayerSelectedIndexChanged(object sender, EventArgs e)
    {
        Picker playerPicker = (Picker)sender;

        if (playerPicker.SelectedIndex < 0) return;

        if (playerPicker.Items.Count != _service.PlayerList.Count)
        {
            // ItemsSource should be updated
            await DisplayAlert("Notification", "Player list is updated. Please select again.", "OK");
            playerPicker.ItemsSource = _service.PlayerList;
            return;
        }

        _service.ChangePlayerDevice(playerPicker.SelectedIndex);
    }


#endif


    private bool TrySetValue<TypeOfValue>(string key, out TypeOfValue state)
    {
        bool result = MyDictionary.TryGetValue(key, out object value);

        state = (TypeOfValue)value;

        return result;
    }


    async private Task QuitAppIfMicPermissionIsNotGranted()
    {
        bool passed = await CheckAndRequestPermission();

        if (passed) return;

        await DisplayAlert("Notification", "App will be quited due to the permission request of microphone is failed.", "OK");

        App.Current.Quit();
    }

    async private Task<bool> CheckAndRequestPermission()
    {
        // Check permission (Mic)
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

        if (status == PermissionStatus.Granted)
            return true;

        //if (Permissions.ShouldShowRationale<Permissions.Microphone>())
        //{
        //    //
        //}

        status = await Permissions.RequestAsync<Permissions.Microphone>();

        if (status == PermissionStatus.Granted)
            return true;

        return false;
    }

    async private void StartButton_Clicked(object sender, EventArgs e)
    {
        // See: https://github.com/dotnet/maui/issues/7377#issuecomment-1311552365
        //await Task.Yield();

        // Start audio recorder & tracker
        //var _service = new VoiceParrotingService(DelayTimeSlider.Value);
        var delay = DelayTimeSlider.Value;
        _service.TrySetDelay(delay);

        bool result = _service.TryStart();
    }

    private void CancelButton_Clicked(object sender, EventArgs e) => _service.Break();


    //async private void RecordButton_Clicked(object sender, EventArgs e)
    //{
    //    await Task.Yield();

    //    //RunProgressBar(VoiceParrotingService.s_recTime);

    //    IsAppBusy = true;
    //    //DisableUIs();
    //    //await Task.DelayInMilli(100);

    //    await _service.RecorderStart();
    //    //EnableUIs();
    //    IsAppBusy = false;
    //}

    //async private void PlayButton_Clicked(object sender, EventArgs e)
    //{
    //    await Task.Yield();

    //    //RunProgressBar(VoiceParrotingService.s_recTime);

    //    IsAppBusy = true;
    //    //DisableUIs();
    //    //await Task.DelayInMilli(100);

    //    await _service.TrackerStart();
    //    //EnableUIs();

    //    IsAppBusy = false;
    //}

    //async private void RunProgressBar(double timeInSec)
    //{
    //    //MyProgress.Progress = 0;
    //    uint milli = (uint)(timeInSec * 1000);
    //    //MyProgress.ProgressTo(1, milli, Easing.Linear);
    //}

    //void DisableUIs()
    //{
    //    //RecordButton.IsEnabled = false;
    //    //PlayButton.IsEnabled = false;
    //    StartButton.IsEnabled = false;

    //    //BusyIndicator.IsVisible = true;
    //    //BusyIndicator.BackgroundColor = Colors.Red;
    //    //BusyIndicator.BackgroundColor = _busyColor;

    //    DelayTimeSlider.IsEnabled = false;
    //}

    //void EnableUIs()
    //{
    //    //RecordButton.IsEnabled = true;
    //    //PlayButton.IsEnabled = true;
    //    StartButton.IsEnabled = true;

    //    //BusyIndicator.IsVisible = false;
    //    //BusyIndicator.BackgroundColor = Colors.LightGreen;
    //    //BusyIndicator.BackgroundColor = _readyColor;

    //    DelayTimeSlider.IsEnabled = true;
    //}

}

