using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;

namespace MauiVoiceParrotingApp.Services;

public partial class VoiceParrotingService
{
    public List<string> RecorderList => _recorder.RecorderList;
    public List<string> PlayerList => _player.PlayerList;

    public void ChangeRecorderDevice(int idx_selected)
    {
        _recorder = new VoiceRecorder(s_samplingFreq, s_recTime, idx_selected);
        _recorder.Initialize();

        UpdateSharedBufferBinding();
    }

    public void ChangePlayerDevice(int idx_selected)
    {
        _player = new VoicePlayer(s_samplingFreq, s_recTime, idx_selected);
        _player.Initialize();

        UpdateSharedBufferBinding();
    }

}
