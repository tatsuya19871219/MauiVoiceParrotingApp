using Android.Media;
using Java.Util;

namespace MauiVoiceParrotingApp.Services;

internal partial class VoicePlayer
{
    private object _sharedBufferLock;

    AudioTrack _audioTrack;
    bool _isInitialized => _audioTrack?.State == AudioTrackState.Initialized;

    readonly int _frames = 10; // [1/sec]
    //int _sharedBufferSize;
    int _miniBufferSize;

    byte[] _sharedBuffer;

    int _playPosition; // in bytes

    public partial void Initialize()
    {
        _miniBufferSize = _sharedBufferSize / _recTime / _frames;

        _audioTrack = new AudioTrack(Android.Media.Stream.Music,
                                        _samplingFreq,
                                        ChannelOut.Mono,
                                        Encoding.Pcm16bit,
                                        _miniBufferSize,
                                        AudioTrackMode.Stream);
    }

    public partial void BindSharedBuffer(VoiceDataSharedBuffer sharedBuffer)
    {
        _sharedBuffer = sharedBuffer.Get<byte[]>();

        _sharedBufferLock = sharedBuffer.Lock;
    }

    public partial async Task Start()
    {
        try
        {
            _audioTrack.Play();
        }
        catch(Exception ex)
        {
            // Illegal State Exception
            return;
        }

        IsRunning = true;

        int location = 0;
        _playPosition = 0;

        byte[] buffer = new byte[_miniBufferSize];

        int maxLocation = _sharedBufferSize / _miniBufferSize;

        while (true)
        {
            if (_audioTrack.PlayState == PlayState.Stopped) break;

            lock (_sharedBufferLock)
            {
                Buffer.BlockCopy(_sharedBuffer, location * _miniBufferSize, buffer, 0, _miniBufferSize);
            }

            int result = await _audioTrack.WriteAsync(buffer, 0, _miniBufferSize);

            location++;

            _playPosition = location * _miniBufferSize;

            if (location >= maxLocation) break;
        }

        End();
    }

    public partial void End()
    {
        try
        {
            _audioTrack.Stop();
        }
        catch (Exception ex)
        {
            // Illegal State Exception
        }

        _playPosition = 0;
        IsRunning = false;

    }

    public partial double GetProgress() => (double)_playPosition / _sharedBufferSize;

    public partial bool CanStart()
    {
        if (_audioTrack == null) return false;

        return !IsRunning & _isInitialized;
    }
    public partial bool CanFinalize()
    {
        if (_audioTrack == null) return false;

        return IsRunning & _isInitialized;
    }
}
