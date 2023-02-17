using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Devices.Sensors;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Windows.Services.Maps;

namespace MauiVoiceParrotingApp.Services;

public partial class VoiceParrotingService
{
    private readonly object _sharedBufferLock = new object();

    private MMDeviceEnumerator _deviceEnum => new MMDeviceEnumerator();
    private List<MMDevice> _recorderDevices => _deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
    private List<MMDevice> _playerDevices => _deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();

    public List<string> RecorderList => _recorderDevices.Select(d => d.ToString()).ToList();
    public List<string> PlayerList => _playerDevices.Select(d => d.ToString()).ToList();

    public void ChangeRecorderDevice(int idx_selected) => PrepareAudioRecorder(idx_selected);
    public void ChangePlayerDevice(int idx_selected) => PrepareAudioTracker(idx_selected);

    IWaveIn _capture;
    IWavePlayer _player;


    int _recLocation = 0;
    //int _recCounter = 0;
    int _playLocation = 0;

    byte[] _sharedBuffer = new byte[s_sharedBufferSize]; // buffer for s_recTime

    BufferedWaveProvider _bufferedWaveProvider; // shared buffer


    public partial double GetRecorderProgress()
    {
        return (double)_recLocation / s_sharedBufferSize;
    }

    public partial double GetTrackerProgress()
    {
        //return (_player as WaveOut).GetPosition() / s_sharedBufferSize;
        return (double)_playLocation / s_sharedBufferSize;
    }

    partial void PrepareAudioRecorder() => PrepareAudioRecorder(0);
    void PrepareAudioRecorder(int idx_selected)
    {
        _capture = new WasapiCapture(_recorderDevices[idx_selected]);


        _capture.DataAvailable += _captureDevice_DataAvailable;

        //_capture.WaveFormat = new WaveFormat(s_samplingFreq, WaveIn.GetCapabilities(idx_selected).Channels); 
        _capture.WaveFormat = new WaveFormat(s_samplingFreq, 1);
    }

    


    private void _captureDevice_DataAvailable(object sender, WaveInEventArgs e)
    {
        int bytesRecorded = e.BytesRecorded;
        byte[] buffer = e.Buffer;

        //int maxLocation = s_sharedBufferSize / s_minBuffSizeInByte;

        lock (_sharedBufferLock)
        {
            //Debug.WriteLineIf(bytesRecorded != s_minBuffSizeInByte, "incomplete buffer size of recode");

            int bytesCopy = bytesRecorded;
            if (_recLocation + bytesRecorded > s_sharedBufferSize) 
                bytesCopy = s_sharedBufferSize - _recLocation;

            Buffer.BlockCopy(buffer, 0, _sharedBuffer, _recLocation, bytesCopy);
            //Buffer.BlockCopy(buffer, 0, _sharedBuffer, _recCounter * s_minBuffSizeInByte, s_minBuffSizeInByte);

            _bufferedWaveProvider.AddSamples(buffer, 0, bytesCopy);

            //_recCounter++;

        }

        _recLocation += bytesRecorded;
        //if (_recCounter >= maxLocation) _capture.StopRecording();
        if (_recLocation >= s_sharedBufferSize) _capture.StopRecording();
    }


    partial void PrepareAudioTracker() => PrepareAudioTracker(0);
    void PrepareAudioTracker(int idx_selected)
    {

        _player = new WasapiOut(_playerDevices[idx_selected], AudioClientShareMode.Shared, false, 0);
        //_player = new WaveOutEvent();

        //IWaveProvider provider = new RawSourceWaveStream(
        //                        new MemoryStream(_sharedBuffer), new WaveFormat(s_samplingFreq, 1));

        _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(s_samplingFreq, 16, 1));

        _bufferedWaveProvider.BufferLength = s_sharedBufferSize;

        _player.Init(_bufferedWaveProvider);

        //_player.Init(provider);

    }


    async public partial Task RecorderStart()
    {

        //_recCounter = 0;
        _recLocation = 0;

        _capture.StartRecording();

        await Task.Delay(s_recTime * 1000);

        //_capture.StopRecording();
    }

    public async partial Task TrackerStart()
    {
        _playLocation = 0;

        _player.Play();

        Task t = Task.Delay(s_recTime * 1000);

        Stopwatch stopwatch= Stopwatch.StartNew();

        while (true)
        {
            double ratio = (double)stopwatch.Elapsed.TotalSeconds / s_recTime;

            _playLocation =  (int)(s_sharedBufferSize * ratio);

            if(t.IsCompleted | _player.PlaybackState == PlaybackState.Stopped) break;

            await Task.Delay(100);
        }

        stopwatch.Stop();
        
    }

    partial void RecorderFinalize()
    {
        _capture.StopRecording();

        _recLocation = 0;
    }

    partial void TrackerFinalize()
    {
        _player.Stop();

        _bufferedWaveProvider.ClearBuffer();

        _playLocation = 0;

        // restore
        //if (_bufferedWaveProvider.BufferedDuration == TimeSpan.Zero)
        //{
        //    _bufferedWaveProvider.AddSamples(_sharedBuffer, 0, s_sharedBufferSize);
        //}
    }

    //public partial void Break()
    //{
    //    _capture.StopRecording();
    //    _player.Stop();

    //    _bufferedWaveProvider.ClearBuffer();

    //    IsRunning = false;
    //}

    /// <summary>
    /// To avoid bufferedWaveProvider reaches full,
    /// add samples dynamically.
    /// </summary>
    //async void RunTracker()
    //{
    //    int location = 0;
    //    int maxLocation = s_sharedBufferSize / s_minBuffSizeInByte;

    //    while (true)
    //    {

    //        lock(_sharedBufferLock)
    //        {
    //            _bufferedWaveProvider.AddSamples(_sharedBuffer, location * s_minBuffSizeInByte, s_minBuffSizeInByte);
    //        }

    //        location++;
    //        if (location >= maxLocation) break;

    //    }

    //}
}
