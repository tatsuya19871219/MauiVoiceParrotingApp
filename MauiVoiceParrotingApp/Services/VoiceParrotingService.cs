using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiVoiceParrotingApp.Services;

public partial class VoiceParrotingService
{
    public VoiceParrotingService(double delay = 1) 
    {
        PrepareAudioRecorder();
        PrepareAudioTracker();
    }

    partial void PrepareAudioRecorder();
    partial void PrepareAudioTracker();

    public partial void Invoke();

    public partial void RecorderInvoke();
    public partial void TrackerInvoke();

}
