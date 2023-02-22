namespace MauiVoiceParrotingApp.Services;

public partial class VoiceParrotingService
{
    public bool IsRunning { get; private set; } = false;
    public bool IsAvailable => _canStart.Value;
    private bool? _canStart => _recorder.CanStart() & _player.CanStart();
    private bool? _canFinalize => _recorder.CanFinalize() & _player.CanFinalize();

    public static int s_samplingFreq = 44100; // [1/sec]

    public static int s_recTime = 10; // [sec]

    //static int s_sharedBufferSize = 2 * s_samplingFreq * s_recTime; // 16bit = 2 * 8bit(1byte)

    private VoiceDataSharedBuffer _sharedBuffer;


    public int DelayInMilli { get; private set; }

    private VoiceRecorder _recorder;
    private VoicePlayer _player;

    public double RecorderProgress => _recorder.GetProgress();
    public double PlayerProgress => _player.GetProgress();

    public bool IsRecorderRunning => _recorder.IsRunning;
    public bool IsPlayerRunning => _player.IsRunning;

    public VoiceParrotingService(double delay = 1)
    {
        TrySetDelay(delay);

        _recorder = new VoiceRecorder(s_samplingFreq, s_recTime);
        _player = new VoicePlayer(s_samplingFreq, s_recTime);

        _recorder.Initialize();
        _player.Initialize();

        UpdateSharedBufferBinding();
    }

    private void UpdateSharedBufferBinding()
    {
        _sharedBuffer = new VoiceDataSharedBuffer();
        _recorder.BindSharedBuffer(_sharedBuffer);
        _player.BindSharedBuffer(_sharedBuffer);
    }


    public bool TrySetDelay(double delay)
    {
        if (IsRunning) return false;

        DelayInMilli = (int)(delay * 1000);
        return true;
    }

    public bool TryStart()
    {
        // Check if recorder/player can start
        if (!_canStart.Value) return false;

        Invoke();
        return true;
    }

    async void Invoke()
    {
        IsRunning = true;

        _recorder.Start();

        await Task.Delay(DelayInMilli);

        await _player.Start();

        //Break();

        IsRunning = false;
    }

    public void Break()
    {
        _recorder.End();
        _player.End();

        IsRunning = false;

        // Tell whether recorder/player could be finalized
        //return _canFinalize.Value;
    }

}
