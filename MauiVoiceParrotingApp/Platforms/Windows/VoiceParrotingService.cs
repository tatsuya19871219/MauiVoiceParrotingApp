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

    IWaveIn _captureDevice;
    IWavePlayer _player;


    int _recCounter = 0;

    //byte[] buffer = new byte[s_minBuffSizeInByte]; 
    
    //static int s_sharedBufferSize = s_minBuffSizeInByte * s_recTime * s_samplingFrame;

    byte[] _sharedBuffer = new byte[s_sharedBufferSize]; // buffer for s_recTime
    //byte[] _sharedBuffer = new byte[s_minBuffSizeInByte * s_recTime * s_samplingFrame]; // buffer for s_recTime

    BufferedWaveProvider _bufferedWaveProvider;

    partial void PrepareAudioRecorder()
    {
        //var devices = Enumerable.Range(-1, WaveIn.DeviceCount + 1).Select(n => WaveIn.GetCapabilities(n)).ToArray();

        //// for test
        int idx_selected = 1;

        //_captureDevice = new WaveInEvent { DeviceNumber = idx_selected };


        // Input device 
        var deviceEnum = new MMDeviceEnumerator();
        var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

        // for test
        _captureDevice = new WasapiCapture(devices[idx_selected]);


        _captureDevice.DataAvailable += _captureDevice_DataAvailable;

        //_captureDevice.WaveFormat = new WaveFormat(s_samplingFreq, WaveIn.GetCapabilities(idx_selected).Channels); 
        _captureDevice.WaveFormat = new WaveFormat(s_samplingFreq, 1);
    }

    int _recLocation = 0;

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
        //if (_recCounter >= maxLocation) _captureDevice.StopRecording();
        if (_recLocation >= s_sharedBufferSize) _captureDevice.StopRecording();
    }

    partial void PrepareAudioTracker()
    {

        // Output device
        var deviceEnum = new MMDeviceEnumerator();
        var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();

        _player = new WasapiOut(devices[1], AudioClientShareMode.Shared, false, 0);
        //_player = new WaveOutEvent();

        //IWaveProvider provider = new RawSourceWaveStream(
        //                        new MemoryStream(_sharedBuffer), new WaveFormat(s_samplingFreq, 1));

        _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(s_samplingFreq, 16, 1));

        _bufferedWaveProvider.BufferLength = s_sharedBufferSize;

        _player.Init(_bufferedWaveProvider);

        //_player.Init(provider);

    }

    public partial void SetDelayTime(double delay) => _delay = delay;

    async public partial Task Invoke()
    {
        _bufferedWaveProvider.ClearBuffer();

        int milli = (int)(_delay * 1000);

        //Task taskRecorder = RecorderInvoke();
        RecorderInvoke();

        await Task.Delay(milli);

        //Task taskTracker = TrackerInvoke();
        await TrackerInvoke();

        //await Task.WhenAll(taskRecorder, taskTracker);

    }

    async public partial Task RecorderInvoke()
    {

        //_recCounter = 0;
        _recLocation = 0;

        _captureDevice.StartRecording();


        await Task.Delay(s_recTime * 1000);


        _captureDevice.StopRecording();
    }

    public async partial Task TrackerInvoke()
    {
        //IWaveProvider provider = new RawSourceWaveStream(
        //                        new MemoryStream(_sharedBuffer), new WaveFormat(s_samplingFreq, 16, 1));

        //int k = WaveOut.GetCapabilities(1).Channels;

        //_player.Init(provider);
        //_player.Init(_bufferedWaveProvider);
        //PrepareAudioTracker();

        // already played
        //if (_bufferedWaveProvider.BufferedDuration == TimeSpan.Zero)
        //{
        //    _bufferedWaveProvider.AddSamples(_sharedBuffer, 0, s_sharedBufferSize);
        //}

        //RunTracker();

        //lock (_sharedBufferLock)
        //{
        _player.Play();
        //}

        await Task.Delay(s_recTime * 1000);

        //while(_player.PlaybackState == PlaybackState.Playing)
        //{
        //    await Task.Delay(500);
        //}

        _player.Stop();

        _bufferedWaveProvider.ClearBuffer();

        //_player.Dispose();

        // restore
        if (_bufferedWaveProvider.BufferedDuration == TimeSpan.Zero)
        {
            _bufferedWaveProvider.AddSamples(_sharedBuffer, 0, s_sharedBufferSize);
        }
    }


    /// <summary>
    /// To avoid bufferedWaveProvider reaches full,
    /// add samples dynamically.
    /// </summary>
    async void RunTracker()
    {
        int location = 0;
        int maxLocation = s_sharedBufferSize / s_minBuffSizeInByte;

        while (true)
        {

            lock(_sharedBufferLock)
            {
                _bufferedWaveProvider.AddSamples(_sharedBuffer, location * s_minBuffSizeInByte, s_minBuffSizeInByte);
            }

            location++;
            if (location >= maxLocation) break;
            
        }

    }
}
