﻿using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MauiVoiceParrotingApp.Services;

internal partial class VoiceRecorder
{
    public bool IsRunning { get; private set; } = false;
    public partial bool CanStart();
    public partial bool CanFinalize();

    int _samplingFreq;
    int _recTime;

    int _sharedBufferSize;

    public VoiceRecorder(int samplingFreq, int recTime)
    {
        _samplingFreq = samplingFreq;
        _recTime = recTime;
        _sharedBufferSize = 2 * samplingFreq * recTime;

        //Initialize();
    }

    public partial void Initialize();
    
    public partial void BindSharedBuffer(VoiceDataSharedBuffer sharedBuffer);

    public partial Task Start();
    public partial void End();
    public partial double GetProgress();
}
