namespace MauiVoiceParrotingApp.Services;

public partial class VoiceParrotingService
{
    public bool IsRunning { get; private set; } = false;

    public static int s_samplingFreq = 44100; // [1/sec]

    public static int s_recTime = 10; // [sec]

    static int s_sharedBufferSize = 2 * s_samplingFreq * s_recTime; // 16bit = 2 * 8bit(1byte)

    double _delay = 0;

    public double Delay => _delay;

    public partial double GetRecorderProgress();
    public partial double GetTrackerProgress();

    public bool IsRecorderRunning { get; private set; } = false;
    public bool IsTrackerRunning { get; private set; } = false;

    public VoiceParrotingService(double delay = 1)
    {
        SetDelayTime(delay);

        PrepareAudioRecorder();
        PrepareAudioTracker();
    }

    public void SetDelayTime(double delay) => _delay = delay;
    partial void PrepareAudioRecorder();
    partial void PrepareAudioTracker();

    public async Task Invoke()
    {
        IsRunning = true;

        int delayInMilli = (int)(_delay * 1000);

        IsRecorderRunning = true;
        RecorderStart();

        await Task.Delay(delayInMilli);

        IsTrackerRunning = true;
        await TrackerStart();

        Break();
    }

    public partial Task RecorderStart();
    public partial Task TrackerStart();

    public void Break()
    {
        RecorderFinalize();
        TrackerFinalize();

        IsRunning = false;
        IsRecorderRunning = false;
        IsTrackerRunning = false;
    }

    partial void RecorderFinalize();
    partial void TrackerFinalize();

}
