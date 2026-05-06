using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;

namespace SpotifyVolumeControl
{
    public class SpotifyVolume(AppSettings _settings)
    {
        private readonly float volumeStep = _settings.VolumeStep;

        public void AdjustVolume(bool volumeUp)
        {
            var spotifySession = FindSpotifySession();
            if (spotifySession == null)
                return;

            float currentVolume = spotifySession.SimpleAudioVolume.Volume;
            float nextVolume = volumeUp ? Math.Min(1.0f, currentVolume + volumeStep) : Math.Max(0.0f, currentVolume - volumeStep);

            spotifySession.SimpleAudioVolume.Volume = nextVolume;
        }

        public static bool IsSpotifyRunning() => FindSpotifySession() != null;

        private static AudioSessionControl? FindSpotifySession()
        {
            try
            {
                using var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                var sessions = device.AudioSessionManager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    var session = sessions[i];
                    try
                    {
                        int pid = (int)session.GetProcessID;
                        var process = Process.GetProcessById(pid);

                        if (process.ProcessName.Equals("spotify", StringComparison.OrdinalIgnoreCase))
                            return session;
                    }
                    catch
                    {
                        //process may have exited between enumeration and lookup
                    }
                }
            }
            catch
            {
                //device enumeration can fail if no audio device is present
            }

            return null;
        }
    }
}