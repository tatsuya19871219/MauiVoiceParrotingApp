using Android.Media;

namespace MauiVoiceParrotingApp.Services;

public partial class VoiceParrotingService
{
    private readonly object _sharedBufferLock = new object();

    AudioRecord _audioRecord;
    AudioTrack _audioTrack;

    int _recPosition; // in bytes
    int _playPosition; // in bytes

    byte[] _sharedBuffer = new byte[s_sharedBufferSize];

    static int s_frames = 10; // [1/sec]
    readonly int _miniBuffSizeInByte = s_sharedBufferSize / s_recTime / s_frames;

    public partial double GetRecorderProgress()
    {
        return (double)_recPosition / s_sharedBufferSize;
    }

    public partial double GetTrackerProgress()
    {
        return (double)_playPosition / s_sharedBufferSize;
    }


    partial void PrepareAudioRecorder()
    {

        _audioRecord = new AudioRecord(AudioSource.Mic,
                                        s_samplingFreq,
                                        ChannelIn.Mono,
                                        Android.Media.Encoding.Pcm16bit,
                                        _miniBuffSizeInByte);

        //_audioRecord.SetPositionNotificationPeriod(1); // # of frames

        //_audioRecord.SetRecordPositionUpdateListener(new MyRecorderUpdatePositionListener(ref _sharedBuffer));

    }

    partial void PrepareAudioTracker()
    {
        _audioTrack = new AudioTrack(Android.Media.Stream.Music,
                                        s_samplingFreq,
                                        ChannelOut.Mono,
                                        Encoding.Pcm16bit,
                                        _miniBuffSizeInByte,
                                        AudioTrackMode.Stream);

        //AudioAttributes attr = new AudioAttributes.Builder()
        //                            .SetLegacyStreamType(Android.Media.Stream.Music)
        //                            .Build();

        //_audioTrack = new AudioTrack(attr, ...)

        //_audioTrack.SetPositionNotificationPeriod(1);

        //_audioTrack.SetPlaybackPositionUpdateListener(new MyTrackerUpdatePositionListener(ref _sharedBuffer));
    }

    //public partial void SetDelayTime(double delay) => _delay = delay;

    async public partial Task RecorderStart()
    {
        //lock(_sharedBufferLock)
        //{
        //    _sharedBuffer = new byte[s_sharedBufferSize];
        //}

        _audioRecord.StartRecording();

        var timerTask =  Task.Delay(s_recTime * 1000);

        int location = 0;
        _recPosition = 0;

        byte[] buffer = new byte[_miniBuffSizeInByte];

        int maxLocation = s_sharedBufferSize / _miniBuffSizeInByte;

        while (true)
        {

            int result = await _audioRecord.ReadAsync(buffer, 0, _miniBuffSizeInByte);

            lock(_sharedBufferLock)
            {
                Buffer.BlockCopy(buffer, 0, _sharedBuffer, location * _miniBuffSizeInByte, _miniBuffSizeInByte);
            }
            location++;

            _recPosition = location * _miniBuffSizeInByte;
            
            if (location >= maxLocation) break;
            if (timerTask.IsCompletedSuccessfully) break;
        }

        //_audioRecord.Stop();
    }


    public async partial Task TrackerStart()
    {
        _audioTrack.Play();

        var timerTask = Task.Delay(s_recTime * 1000);

        int location = 0;
        _playPosition = 0;

        byte[] buffer = new byte[_miniBuffSizeInByte];

        int maxLocation = s_sharedBufferSize / _miniBuffSizeInByte;

        while (true)
        {
            lock(_sharedBufferLock)
            {
                Buffer.BlockCopy(_sharedBuffer, location * _miniBuffSizeInByte, buffer, 0, _miniBuffSizeInByte);
            }

            int result = await _audioTrack.WriteAsync(buffer, 0, _miniBuffSizeInByte);

            location++;

            _playPosition = location * _miniBuffSizeInByte;

            if (location >= maxLocation) break;
            if (timerTask.IsCompletedSuccessfully) break;
        }

        //_audioTrack.Stop();
    }


    partial void RecorderFinalize()
    {
        _audioRecord.Stop();
    }
    partial void TrackerFinalize()
    {
        _audioTrack.Stop();
    }


    //
    //class MyRecorderUpdatePositionListener : Java.Lang.Object, AudioRecord.IOnRecordPositionUpdateListener
    //{
    //    byte[] _sharedBuffer;

    //    public MyRecorderUpdatePositionListener(ref byte[] sharedBuffer) 
    //    {
    //        _sharedBuffer = sharedBuffer;
    //    }


    //    public void OnMarkerReached(AudioRecord recorder)
    //    {
    //        // nop
    //        //throw new NotImplementedException();
    //    }

    //    public void OnPeriodicNotification(AudioRecord recorder)
    //    {
    //        byte[] buffer = new byte[recorder.BufferSizeInFrames];

    //        recorder.Read(buffer, 0, buffer.Length);

    //        Buffer.BlockCopy(buffer, 0, _sharedBuffer, 
    //                        recorder.NotificationMarkerPosition * recorder.BufferSizeInFrames, 
    //                        _miniBuffSizeInByte);

    //        //throw new NotImplementedException();
    //    }
    //}

    //class MyTrackerUpdatePositionListener : Java.Lang.Object, AudioTrack.IOnPlaybackPositionUpdateListener
    //{
    //    byte[] _sharedBuffer;

    //    public MyTrackerUpdatePositionListener(ref byte[] sharedBuffer)
    //    {
    //        _sharedBuffer = sharedBuffer;
    //    }


    //    public void OnMarkerReached(AudioTrack track)
    //    {
    //        //throw new NotImplementedException();
    //    }

    //    public void OnPeriodicNotification(AudioTrack track)
    //    {
    //        //throw new NotImplementedException();
    //    }
    //}
}
