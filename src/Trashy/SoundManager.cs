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

        private static readonly Queue<AudioSource> s_queue = new Queue<AudioSource>();
        private static readonly List<AudioSource> s_active = new List<AudioSource>();
        private static AudioClip s_audioClip;
        private static float s_lastPlay;

        public static void Play()
        {
            // Sounds better if hit sounds are not playing exactly at the same time
            if (Time.realtimeSinceStartup - s_lastPlay < 0.05f)
                return;

            if (s_queue.Count > 0 && s_audioClip != null)
            {
                var audioSource = s_queue.Dequeue();
                audioSource.clip = s_audioClip;
                audioSource.volume = ConfigManager.HitSoundVolume.Value / 100f;
                audioSource.Play();
                s_active.Add(audioSource);
                s_lastPlay = Time.realtimeSinceStartup;
            }
        }

        public static async Task LoadAudioClip()
        {
            foreach (var audioSource in s_active)
            {
                audioSource.Stop();
                audioSource.clip = null;
                s_queue.Enqueue(audioSource);
            }

            s_active.Clear();

            if (s_audioClip != null)
            {
                AudioClip.Destroy(s_audioClip);
                s_audioClip = null;
            }

            var fileName = Path.Combine(Paths.PluginPath, "Trashy", "hit.mp3");
            if (!File.Exists(fileName))
                return;

            using (var request = UnityWebRequestMultimedia.GetAudioClip(
                       new Uri(fileName),
                       AudioType.MPEG
                   ))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                    Log.Error<SoundManager>("Unable to load hit.mp3");
                else
                    s_audioClip = DownloadHandlerAudioClip.GetContent(request);
            }
        }

        private async void Start()
        {
            await LoadAudioClip();
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
    }
}
