

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

    byte[] _sahredBuffer = new byte[s_sharedBufferSize]; // buffer for s_recTime
    //byte[] _sahredBuffer = new byte[s_minBuffSizeInByte * s_recTime * s_samplingFrame]; // buffer for s_recTime

    partial void PrepareAudioRecorder()
    {
        //var devices = Enumerable.Range(-1, WaveIn.DeviceCount + 1).Select(n => WaveIn.GetCapabilities(n)).ToArray();

        //// for test
        int idx_selected = 1;

        //_captureDevice = new WaveInEvent { DeviceNumber = idx_selected };


        // 
        var deviceEnum = new MMDeviceEnumerator();
        var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

        // for test
        _captureDevice = new WasapiCapture(devices[idx_selected]);


        _captureDevice.DataAvailable += _captureDevice_DataAvailable;

        _captureDevice.WaveFormat = new WaveFormat(s_samplingFreq, WaveIn.GetCapabilities(idx_selected).Channels); 
        //_captureDevice.WaveFormat = new WaveFormat(s_samplingFreq, 1);
    }

    private void _captureDevice_DataAvailable(object sender, WaveInEventArgs e)
    {
        int bytesRecorded = e.BytesRecorded;
        byte[] buffer = e.Buffer;

        int maxLocation = s_sharedBufferSize / s_minBuffSizeInByte;

        lock (_sharedBufferLock)
        {
            //Buffer.BlockCopy(buffer, 0, _sahredBuffer, _recCounter * bytesRecorded, bytesRecorded);
            Buffer.BlockCopy(buffer, 0, _sahredBuffer, _recCounter * s_minBuffSizeInByte, s_minBuffSizeInByte);
            _recCounter++;

        }

        if (_recCounter >= maxLocation) _captureDevice.StopRecording();
    }

    partial void PrepareAudioTracker()
    {
        _player = new WaveOutEvent();

        //IWaveProvider provider = new RawSourceWaveStream(
        //                        new MemoryStream(_sahredBuffer), new WaveFormat(s_samplingFreq, 1));

        //var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(s_samplingFreq, 16, 1));



        //_player.Init(provider);

    }

    public partial void SetDelayTime(double delay) => _delay = delay;

    async public partial Task Invoke()
    {
        int milli = (int)(_delay * 1000);

        //Task taskRecorder = RecorderInvoke();
        Task.Run(RecorderInvoke);

        await Task.Delay(milli);

        //Task taskTracker = TrackerInvoke();
        await Task.Run(TrackerInvoke);

        //await Task.WhenAll(taskRecorder, taskTracker);

    }

    async public partial Task RecorderInvoke()
    {

        _recCounter = 0;

        _captureDevice.StartRecording();


        await Task.Delay(s_recTime * 1000);


        _captureDevice.StopRecording();
    }

    public async partial Task TrackerInvoke()
    {
        IWaveProvider provider = new RawSourceWaveStream(
                                new MemoryStream(_sahredBuffer), new WaveFormat(s_samplingFreq, 2));

        _player.Init(provider);

        _player.Play();

        await Task.Delay(s_recTime * 1000);

        _player.Stop();

    }
}
