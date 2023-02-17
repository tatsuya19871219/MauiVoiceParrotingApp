using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiVoiceParrotingApp.Services;

public partial class VoiceParrotingService
{

    public static int s_samplingFreq = 44100; // [1/sec]

    //static int s_samplingFrame = 10;

    static int s_minBuffSizeInByte = 2 * s_samplingFreq; // 16bit = 2 * 8bit(1byte)

    public static int s_recTime = 10; // [sec]

    static int s_sharedBufferSize = s_minBuffSizeInByte * s_recTime;

    double _delay = 0;

    public VoiceParrotingService(double delay = 1) 
    {
        PrepareAudioRecorder();
        PrepareAudioTracker();
    }

    partial void PrepareAudioRecorder();
    partial void PrepareAudioTracker();

    public partial Task Invoke();

    public partial void SetDelayTime(double delay);

    public partial Task RecorderInvoke();
    public partial Task TrackerInvoke();

}
