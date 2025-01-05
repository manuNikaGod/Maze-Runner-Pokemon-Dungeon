using NAudio.Wave;

public class MusicPlayer
{
    private IWavePlayer waveOutDevice;
    private AudioFileReader audioFileReader;

    public void PlayMusic(string filePath)
    {
        waveOutDevice = new WaveOut();
        audioFileReader = new AudioFileReader(filePath);
        waveOutDevice.Init(audioFileReader);
        waveOutDevice.Play();

    
        waveOutDevice.PlaybackStopped += OnPlaybackStopped;
    }

    private void OnPlaybackStopped(object sender, StoppedEventArgs args)
    {
        audioFileReader.Position = 0;
        waveOutDevice.Play();
    }

    public void StopMusic()
    {
        waveOutDevice.Stop();
        audioFileReader.Dispose();
        waveOutDevice.Dispose();
    }
}
