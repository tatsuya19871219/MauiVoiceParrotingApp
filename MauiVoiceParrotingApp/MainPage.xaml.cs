using MauiVoiceParrotingApp.Services;

namespace MauiVoiceParrotingApp;

public partial class MainPage : ContentPage
{

    VoiceParrotingService service = new();

    public MainPage()
    {
        InitializeComponent();
    }

    private void StartButton_Clicked(object sender, EventArgs e)
    {
        // Start audio recorder & tracker
        //var service = new VoiceParrotingService(DelayTimeSlider.Value);

        service.Invoke();
    }

    private void RecordButton_Clicked(object sender, EventArgs e)
    {
        service.RecorderInvoke();
    }

    private void PlayButton_Clicked(object sender, EventArgs e)
    {
        service.TrackerInvoke();
    }
}