using Android.Media;
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
    AudioRecord _audioRecord;
    AudioTrack _audioTrack;

    static int s_samplingFreq = 44100; // [1/sec]

    static int s_samplingFrame = 10;

    static int s_minBuffSizeInByte = 2 * s_samplingFreq / s_samplingFrame; // 16bit = 2 * 8bit(1byte)

    static int s_recTime = 10; // [sec]

    //byte[] buffer = new byte[s_minBuffSizeInByte]; 

    byte[] _sahredBuffer = new byte[s_minBuffSizeInByte * s_recTime]; // buffer for s_recTime

    partial void PrepareAudioRecorder()
    {

        _audioRecord = new AudioRecord(AudioSource.Mic,
                                        s_samplingFreq,
                                        ChannelIn.Mono,
                                        Android.Media.Encoding.Pcm16bit,
                                        s_minBuffSizeInByte);
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
    }

    async public partial void Invoke()
    {
        
    }

    async public partial void RecorderInvoke()
    {
        _audioRecord.StartRecording();

        var timerTask =  Task.Delay(s_recTime * 1000);

        int location = 0;

        byte[] buffer = new byte[s_minBuffSizeInByte];


        while (true)
        {

            _audioRecord.Read(buffer, 0, s_minBuffSizeInByte);

            //Buffer.BlockCopy(buffer, 0, _sahredBuffer, location*buffer.Length, buffer.Length);

            location++;

            if (timerTask.IsCompletedSuccessfully) break;
        }

        _audioRecord.Stop();
    }

    public async partial void TrackerInvoke()
    {
        _audioTrack.Play();

        var timerTask = Task.Delay(s_recTime * 1000);

        int location = 0;

        while (true)
        {
            _audioTrack.Write(_sahredBuffer, location*s_minBuffSizeInByte, s_minBuffSizeInByte);

            location++;

            if (timerTask.IsCompletedSuccessfully) break;
        }

        _audioTrack.Stop();
    }
}
