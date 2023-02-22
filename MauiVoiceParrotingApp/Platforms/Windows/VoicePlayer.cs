using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiVoiceParrotingApp.Services;

internal partial class VoicePlayer //: IDisposable
{
    private object _sharedBufferLock;

    private MMDeviceEnumerator _deviceEnum => new MMDeviceEnumerator();
    private List<MMDevice> _playerDevices => _deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
    public List<string> PlayerList => _playerDevices.Select(d => d.ToString()).ToList();

    //public void ChangePlayerDevice(int idx_selected) => PrepareAudioTracker(idx_selected);


    IWavePlayer _audioTrack;

    bool _isInitialized = false;
    bool _isBoundBuffer = false;

    int _playPosition; // in bytes

    int _cuurentDeviceIndex = 0;

    BufferedWaveProvider _sharedBuffer;


    public VoicePlayer(int samplingFreq, int recTime, int deviceIndex) : this(samplingFreq, recTime)
    {
        _cuurentDeviceIndex = deviceIndex;
    }

    public partial void Initialize()
    {
        _audioTrack = new WasapiOut(_playerDevices[_cuurentDeviceIndex], 
                                        AudioClientShareMode.Shared, false, 0);

        _audioTrack.PlaybackStopped += PlaybackStopped;

        _isInitialized = true;
    }


    private void PlaybackStopped(object sender, StoppedEventArgs e)
    {
        IsRunning = false;
    }

    public partial void BindSharedBuffer(VoiceDataSharedBuffer sharedBuffer)
    {
        _sharedBuffer = sharedBuffer.Get<BufferedWaveProvider>();

        _sharedBufferLock = sharedBuffer.Lock;

        if (_isBoundBuffer) Initialize();

        _audioTrack.Init(_sharedBuffer); // multiple call of Init causes some error

        _isBoundBuffer = true;

    }

    public partial async Task Start()
    {
        if(!CanStart()) return;

        _audioTrack.Play();

        _playPosition = 0;

        IsRunning = true;

        Stopwatch stopwatch = Stopwatch.StartNew();

        int bytesParSecond = _sharedBufferSize / _recTime;

        while (true)
        {
            await Task.Delay(100);

            // update play position
            _playPosition = (int) (bytesParSecond * stopwatch.Elapsed.TotalSeconds);

            if (_playPosition > _sharedBufferSize)
            {
                _playPosition = _sharedBufferSize;
                End(); 
                break;
            }

            if (!IsRunning)
            {
                _playPosition = 0;
                break;
            }
        }

        //IsRunning = false;
    }

    public partial void End()
    {
        if (!CanFinalize()) return;
        
        _audioTrack.Stop(); 

        _playPosition = 0;
        _sharedBuffer.ClearBuffer();
    }

    public partial double GetProgress() => (double)_playPosition / _sharedBufferSize;

    public partial bool CanStart()
    {
        if (_audioTrack == null) return false;

        return !IsRunning & _isBoundBuffer;
    }
    public partial bool CanFinalize()
    {
        if (_audioTrack == null) return false;

        return IsRunning & _isInitialized;
    }


    //public void Dispose()
    //{
    //    _sharedBuffer = null;
    //    _audioTrack?.Dispose();
    //}

    //~VoicePlayer()
    //{
    //    _audioTrack.Dispose();
    //    _audioTrack = null;
    //}

}
