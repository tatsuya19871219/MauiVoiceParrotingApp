using Android.Media;
using Android.Renderscripts;
using Android.Speech;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security;
//using System.Text;
//using System.Threading.Tasks;
//using Xamarin.Google.Crypto.Tink;

namespace MauiVoiceParrotingApp.Services;

public partial class VoiceParrotingService
{
    private readonly object _sharedBufferLock = new object();

    AudioRecord _audioRecord;
    AudioTrack _audioTrack;

    

    //byte[] buffer = new byte[s_minBuffSizeInByte]; 

    //static int s_sharedBufferSize = s_minBuffSizeInByte * s_recTime * s_samplingFrame;

    byte[] _sharedBuffer = new byte[s_sharedBufferSize]; // buffer for s_recTime

    partial void PrepareAudioRecorder()
    {

        _audioRecord = new AudioRecord(AudioSource.Mic,
                                        s_samplingFreq,
                                        ChannelIn.Mono,
                                        Android.Media.Encoding.Pcm16bit,
                                        s_minBuffSizeInByte);

        //_audioRecord.SetPositionNotificationPeriod(1); // # of frames

        //_audioRecord.SetRecordPositionUpdateListener(new MyRecorderUpdatePositionListener(ref _sharedBuffer));

    }

    partial void PrepareAudioTracker()
    {
        _audioTrack = new AudioTrack(Android.Media.Stream.Music,
                                        s_samplingFreq,
                                        ChannelOut.Mono,
                                        Encoding.Pcm16bit,
                                        s_minBuffSizeInByte,
                                        AudioTrackMode.Stream);

        //AudioAttributes attr = new AudioAttributes.Builder()
        //                            .SetLegacyStreamType(Android.Media.Stream.Music)
        //                            .Build();

        //_audioTrack = new AudioTrack(attr, ...)

        //_audioTrack.SetPositionNotificationPeriod(1);

        //_audioTrack.SetPlaybackPositionUpdateListener(new MyTrackerUpdatePositionListener(ref _sharedBuffer));
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
        //lock(_sharedBufferLock)
        //{
        //    _sharedBuffer = new byte[s_sharedBufferSize];
        //}

        _audioRecord.StartRecording();

        var timerTask =  Task.Delay(s_recTime * 1000);

        int location = 0;

        byte[] buffer = new byte[s_minBuffSizeInByte];

        int maxLocation = s_sharedBufferSize / s_minBuffSizeInByte;

        while (true)
        {

            int result = await _audioRecord.ReadAsync(buffer, 0, s_minBuffSizeInByte);

            lock(_sharedBufferLock)
            {
                Buffer.BlockCopy(buffer, 0, _sharedBuffer, location * s_minBuffSizeInByte, s_minBuffSizeInByte);
            }
            location++;

            if (location >= maxLocation) break;
            if (timerTask.IsCompletedSuccessfully) break;
        }

        _audioRecord.Stop();
    }


    public async partial Task TrackerInvoke()
    {
        _audioTrack.Play();

        var timerTask = Task.Delay(s_recTime * 1000);

        int location = 0;

        byte[] buffer = new byte[s_minBuffSizeInByte];

        int maxLocation = s_sharedBufferSize / s_minBuffSizeInByte;

        while (true)
        {
            lock(_sharedBufferLock)
            {
                Buffer.BlockCopy(_sharedBuffer, location * s_minBuffSizeInByte, buffer, 0, s_minBuffSizeInByte);
            }

            int result = await _audioTrack.WriteAsync(buffer, 0, s_minBuffSizeInByte);

            location++;

            if (location >= maxLocation) break;
            if (timerTask.IsCompletedSuccessfully) break;
        }

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
    //                        s_minBuffSizeInByte);

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
