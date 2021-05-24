﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using LLHandlers;
using LLScreen;

namespace MusicHax
{
    public class MusicHax : MonoBehaviour
    {
        public static MusicHax instance = null;
        private static readonly string MHResourcesPath = Path.Combine(Path.Combine(Application.dataPath, "Managed"), "MusicHaxResources");
        private static Dictionary<string, List<AudioClip>> musicCache = new Dictionary<string, List<AudioClip>>();

        public static void Initialize()
        {
            GameObject gameObject = new GameObject("MusicHax");
            instance = gameObject.AddComponent<MusicHax>();
            DontDestroyOnLoad(gameObject);
        }

        private bool loadingExternalMusicFiles = false;
        private readonly string loadingText = $"MusicHax is loading External Songs...";

        // Awake is called once when both the game libs and the plug-in are loaded
        void Awake()
        {
            Directory.CreateDirectory(MHResourcesPath);
            this.StartCoroutine(LoadMusics());
        }

        private void OnGUI()
        {
            var OriginalColor = GUI.contentColor;
            var OriginalLabelFontSize = GUI.skin.label.fontSize;
            var OriginalLabelAlignment = GUI.skin.label.alignment;

            GUI.contentColor = Color.white;
            GUI.skin.label.fontSize = 50;
            if (UIScreen.loadingScreenActive && this.loadingExternalMusicFiles == true)
            {
                GUIStyle label = new GUIStyle(GUI.skin.label);
                var sX = Screen.width / 2;
                var sY = UIScreen.GetResolutionFromConfig().height / 3;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, sY + 50, Screen.width, sY), loadingText);
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            GUI.contentColor = OriginalColor;
            GUI.skin.label.fontSize = OriginalLabelFontSize;
            GUI.skin.label.alignment = OriginalLabelAlignment;

        }

        private IEnumerator LoadMusics()
        {
            musicCache.Clear();
            this.loadingExternalMusicFiles = true;
            UIScreen.SetLoadingScreen(true, false, false, Stage.NONE);
            DirectoryInfo[] musicDirectories = new DirectoryInfo(MHResourcesPath).GetDirectories();
            foreach (DirectoryInfo musicDirectory in musicDirectories)
            {
                List<AudioClip> audioClips = new List<AudioClip>();
                FileInfo[] musicFiles = musicDirectory.GetFiles("*.ogg");

                foreach (FileInfo musicFile in musicFiles)
                {
                    Debug.Log("MusicHax: Loading new " + musicDirectory.Name + " : " + musicFile.FullName);
                    using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + musicFile.FullName, AudioType.OGGVORBIS))
                    {
                        UnityWebRequestAsyncOperation asyncOperation = null;
                        try
                        {
                            asyncOperation = uwr.SendWebRequest();

                        }
                        catch (Exception e)
                        {
                            Debug.LogError("MusicHax: Exception caught: ");
                            Debug.LogException(e);
                            continue;
                        }
                        yield return asyncOperation;
                        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);

                        if (audioClip == null)
                        {
                            Debug.LogError("Huh?!");
                        }
                        else
                        {
                            audioClip.name = Path.GetFileNameWithoutExtension(musicFile.FullName);
                            audioClips.Add(audioClip);
                        }
                    }
                }
                musicCache.Add(musicDirectory.Name, audioClips);
            }

            UIScreen.SetLoadingScreen(false);
            loadingExternalMusicFiles = false;
            yield break;
        }


        private void ReloadMusics()
        {
            LoadMusics();
        }

        public static void CreateMusicDirectory(string clipName)
        {
            Directory.CreateDirectory(Path.Combine(MHResourcesPath, clipName));
        }


        private static AudioClip GetClipNow(string musicFilePath)
        {
            Debug.Log("MusicHax: Urgent File Load: " + musicFilePath);
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + musicFilePath, AudioType.OGGVORBIS))
            {
                try
                {
                    uwr.SendWebRequest();
                }
                catch (Exception e)
                {
                    Debug.LogError("MusicHax: Exception caught: ");
                    Debug.LogException(e);
                }
                while (!uwr.isDone)
                {
                    Thread.Sleep(20);
                }
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);

                if (audioClip == null)
                {
                    Debug.LogError("Huh?!");
                }
                else
                {
                    audioClip.name = Path.GetFileNameWithoutExtension(musicFilePath);
                    return audioClip;
                }
            }
            return null;
        }

        public static AudioClip GetAudioClipFor(string clipName)
        {
            Debug.Log("MusicHax: Got asked for a clip named: \"" + clipName + "\"");
            if (musicCache.ContainsKey(clipName) && musicCache[clipName].Count > 0)
            {
                return musicCache[clipName][UnityEngine.Random.Range(0, musicCache[clipName].Count)];
            }
            else
            {
                try
                {
                    string[] pathList = Directory.GetFiles(Path.Combine(MHResourcesPath, clipName));
                    if (pathList.Length > 0)
                    {
                        return GetClipNow(pathList[UnityEngine.Random.Range(0, pathList.Length - 1)]);
                    }
                } catch (Exception e)
                {
                    Debug.LogError("MusicHax: Exception caught: ");
                    Debug.LogException(e);
                }
                return null;
            }
        }

        private const string modVersion = "v2.0";
        private const string repositoryOwner = "PKPenguin321";
        private const string repositoryName = "LLBMusicHax";

    }
}