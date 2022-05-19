using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace Trashy
{
    public class SoundManager : MonoBehaviour
    {
        private const int MaxSounds = 6;

        private static readonly List<AudioClip> s_audioClips = new List<AudioClip>();
        private static readonly Queue<AudioSource> s_queue = new Queue<AudioSource>();
        private static readonly List<AudioSource> s_active = new List<AudioSource>();
        private static float s_lastPlay;

        public static void Play()
        {
            // Sounds better if hit sounds are not playing exactly at the same time
            if (Time.realtimeSinceStartup - s_lastPlay < 0.05f)
                return;

            if (s_queue.Count > 0 && s_audioClips.Count > 0)
            {
                // Pick audio clip at random
                var audioClip = s_audioClips.Random();
                var audioSource = s_queue.Dequeue();
                audioSource.clip = audioClip;
                audioSource.volume = ConfigManager.HitSoundVolume.Value / 100f;
                audioSource.Play();
                s_active.Add(audioSource);
                s_lastPlay = Time.realtimeSinceStartup;
            }
        }

        private static async Task<AudioClip> LoadClip(string fileName)
        {
            // Get audio type from file name
            var audioType = AudioTypeForFile(fileName);
            if (audioType == AudioType.UNKNOWN)
            {
                Log.Error<SoundManager>($"Unable to detect format for audio file: {fileName}");
                return null;
            }

            // Load file
            using (var request = UnityWebRequestMultimedia.GetAudioClip(new Uri(fileName), audioType))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log.Error<SoundManager>($"Unable to load audio file: {fileName}");
                    return null;
                }

                return DownloadHandlerAudioClip.GetContent(request);
            }
        }

        public static async Task LoadAudioClips()
        {
            foreach (var audioSource in s_active)
            {
                audioSource.Stop();
                audioSource.clip = null;
                s_queue.Enqueue(audioSource);
            }

            s_active.Clear();

            // Unload all audio clips
            if (s_audioClips.Count > 0)
            {
                foreach (var audioClip in s_audioClips)
                    AudioClip.Destroy(audioClip);

                s_audioClips.Clear();
            }

            var soundsDirectory = Path.Combine(Paths.PluginPath, "Trashy", "Sounds");
            foreach (var file in Directory.GetFiles(soundsDirectory, "*.*"))
            {
                // Load audio clip and add it to the list
                var clip = await LoadClip(file);
                if (clip)
                    s_audioClips.Add(clip);
            }
        }

        private async void Start()
        {
            await LoadAudioClips();
            for (var i = 0; i < MaxSounds; ++i)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                s_queue.Enqueue(audioSource);
            }
        }

        private void Update()
        {
            IEnumerable<AudioSource> active = s_active;
            foreach (var audioSource in active.Reverse())
            {
                if (!audioSource.isPlaying)
                {
                    s_active.Remove(audioSource);
                    s_queue.Enqueue(audioSource);
                }
            }
        }

        private static AudioType AudioTypeForFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            switch (extension.ToLower())
            {
                case ".wav":
                    return AudioType.WAV;

                case ".mp3":
                    return AudioType.MPEG;

                case ".ogg":
                    return AudioType.OGGVORBIS;

                case ".aiff":
                case ".aif":
                    return AudioType.AIFF;

                default:
                    return AudioType.UNKNOWN;
            }
        }
    }
}
