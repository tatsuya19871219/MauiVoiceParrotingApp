using Android.Media;
using System.Threading.Tasks;

namespace MauiVoiceParrotingApp.Services;

internal partial class VoiceRecorder
{
    private object _sharedBufferLock;

    AudioRecord _audioRecord;
    bool _isInitialized => _audioRecord?.State == State.Initialized;

    readonly int _frames = 10; // [1/sec]
    //int _sharedBufferSize;
    int _miniBufferSize;

    byte[] _sharedBuffer;

    int _recPosition; // in bytes

    public partial void Initialize()
    {
        _miniBufferSize = _sharedBufferSize / _recTime / _frames;

        _audioRecord = new AudioRecord(AudioSource.Mic,
                                        _samplingFreq,
                                        ChannelIn.Mono,
                                        Android.Media.Encoding.Pcm16bit,
                                        _miniBufferSize);
    }
    
    public partial void BindSharedBuffer(VoiceDataSharedBuffer sharedBuffer)
    {
        _sharedBuffer = new byte[_sharedBufferSize];

        sharedBuffer.Set<byte[]>(_sharedBuffer);

        _sharedBufferLock = sharedBuffer.Lock;
    }

    public partial async Task Start()
    {
        try
        {
            _audioRecord.StartRecording();
        }
        catch(Exception ex)
        {
            // Illegal State Exception
            return;
        }

        IsRunning = true;

        int location = 0;
        _recPosition = 0;

        byte[] buffer = new byte[_miniBufferSize];

        int maxLocation = _sharedBufferSize / _miniBufferSize;

        while (true)
        {
            if (_audioRecord.RecordingState == RecordState.Stopped) break;

            int result = await _audioRecord.ReadAsync(buffer, 0, _miniBufferSize);

            lock(_sharedBufferLock)
            {
                Buffer.BlockCopy(buffer, 0, _sharedBuffer, location*_miniBufferSize, _miniBufferSize);
            }

            location++;

            _recPosition = location * _miniBufferSize;

            if (location >= maxLocation) break;
        }

        End();

    }

    public partial void End()
    {
        try
        {
            _audioRecord.Stop();
        }
        catch(Exception ex)
        {
            // Illegal State Exception
        }

        _recPosition = 0;
        IsRunning = false;
    }

    public partial double GetProgress() => (double)_recPosition / _sharedBufferSize;

    public partial bool CanStart()
    {
        if (_audioRecord == null ) return false;

        return !IsRunning & _isInitialized;
    }
    public partial bool CanFinalize()
    {
        if (_audioRecord == null) return false;

        return IsRunning & _isInitialized;
    }
}
