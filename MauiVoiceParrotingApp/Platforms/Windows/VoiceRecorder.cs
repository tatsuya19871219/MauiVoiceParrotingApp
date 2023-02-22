using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace MauiVoiceParrotingApp.Services;

internal partial class VoiceRecorder //: IDisposable
{
    private object _sharedBufferLock;

    private MMDeviceEnumerator _deviceEnum => new MMDeviceEnumerator();
    private List<MMDevice> _recorderDevices => _deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
    public List<string> RecorderList => _recorderDevices.Select(d => d.ToString()).ToList();
    //public void ChangeRecorderDevice(int idx_selected) => PrepareAudioRecorder(idx_selected);

    IWaveIn _audioRecord;

    bool _isInitialized = false;
    bool _isBoundBuffer = false;

    int _recPosition; // in bytes

    int _currentDeviceIndex = 0;

    //public int DeviceIndex { get; init; } = -1;

    BufferedWaveProvider _sharedBuffer;


    public VoiceRecorder(int samplingFreq, int recTime, int deviceIndex) : this(samplingFreq, recTime)
    {
        _currentDeviceIndex = deviceIndex;
    }

    public partial void Initialize()
    {
        _audioRecord = new WasapiCapture(_recorderDevices[_currentDeviceIndex]);

        _audioRecord.DataAvailable += DataAvailable;

        _audioRecord.RecordingStopped += RecordingStopped;

        _audioRecord.WaveFormat = new WaveFormat(_samplingFreq, 1);

        _isInitialized = true;
    }


    private void DataAvailable(object sender, WaveInEventArgs e)
    {
        int bytesRecorded = e.BytesRecorded;
        byte[] buffer = e.Buffer;

        int bytesCopy = bytesRecorded;
        if (_recPosition + bytesRecorded > _sharedBufferSize)
            bytesCopy = _sharedBufferSize - _recPosition;

        _sharedBuffer.AddSamples(buffer, 0, bytesCopy);

        _recPosition += bytesCopy;

        if (_recPosition >= _sharedBufferSize) End(); //_audioRecord.StopRecording();
    }

    private void RecordingStopped(object sender, StoppedEventArgs e)
    {
        IsRunning = false;
    }
    
    public partial void BindSharedBuffer(VoiceDataSharedBuffer sharedBuffer)
    {
        _sharedBuffer = new BufferedWaveProvider(_audioRecord.WaveFormat);

        _sharedBuffer.BufferLength = _sharedBufferSize;

        sharedBuffer.Set<BufferedWaveProvider>(_sharedBuffer);

        _sharedBufferLock = sharedBuffer.Lock;

        _isBoundBuffer = true;
    }

    public partial async Task Start()
    {
        if (!CanStart()) return;

        _audioRecord.StartRecording();

        _recPosition = 0;

        IsRunning = true;

        while (true)
        {
            await Task.Delay(100);
            if (!IsRunning) break;
        }
    }
    public partial void End()
    {
        if (!CanFinalize()) return;
         
        _audioRecord.StopRecording();

        _recPosition = 0;
        //_sharedBuffer.ClearBuffer();
    }
    public partial double GetProgress() => (double)_recPosition / _sharedBufferSize;

    public partial bool CanStart()
    {
        if (_audioRecord == null) return false;

        return !IsRunning & _isBoundBuffer;
    }
    public partial bool CanFinalize()
    {
        if (_audioRecord == null) return false;

        return IsRunning & _isInitialized;
    }


    //public void Dispose()
    //{
    //    _sharedBuffer = null;
    //    _audioRecord?.Dispose();
    //}

    //~VoiceRecorder()
    //{
    //    _audioRecord.Dispose();
    //}
}
