﻿using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;

namespace MusicHax
{
    public class MusicHax : MonoBehaviour
    {
        public static MusicHax instance = null;

        public ModMenuIntegration MMI = null;

        public static void Initialize()
        {
            GameObject gameObject = new GameObject("MusicHax");
            instance = gameObject.AddComponent<MusicHax>();
            DontDestroyOnLoad(gameObject);
        }

        public static void PlayHaxedMusic(ref AudioClip clip)
        {
            // Path.Combine for linux, old .Net version so you have to chain them
            string audioClipPath = Path.Combine(Path.Combine(Path.Combine(Application.dataPath, "Managed"), "MusicHaxResources"), clip.name);
            Directory.CreateDirectory(audioClipPath);
            FileInfo[] customSongFiles = new DirectoryInfo(audioClipPath).GetFiles("*.ogg");
            if (customSongFiles.Length != 0)
            {
                int index = new System.Random().Next(0, customSongFiles.Length);
                string newSongName = customSongFiles[index].Name;
                Resources.Load<AudioClip>(customSongFiles[index].FullName);
                UnityEngine.Debug.Log("MUSICHAX: loading newSongName as " + newSongName + ", at FullName " + customSongFiles[index].FullName);
                // I couldn't find how to reference WWW and it was marked as deprecated so i went with that
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(customSongFiles[index].FullName, AudioType.OGGVORBIS)) {
                    www.SendWebRequest();
                    while (!www.isDone)
                    {
                        Thread.Sleep(50);
                    }
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip == null) {
                        Debug.LogError("MUSICHAX: AudioClip audioClip failed to GetContent(www)");
                    } else {
                        clip = audioClip;
                        clip.name = Path.GetFileNameWithoutExtension(newSongName);
                    }
                }
            }
        }
    }
}