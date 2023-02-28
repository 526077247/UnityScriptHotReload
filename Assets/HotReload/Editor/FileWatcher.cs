using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using static ScriptHotReload.HotReloadConfig;

namespace ScriptHotReload
{
    /// <summary>
    /// ����cs�ļ��仯
    /// </summary>
    [InitializeOnLoad]
    public class FileWatcher
    {
        public class FileEntry
        {
            public string   path;
            public int      version;
            public DateTime lastModify;
        }

        public static bool hasChangedSinceLast { get; private set; } = false;
        public static DateTime lastModifyTime { get; private set; } = DateTime.MinValue;
        public static Dictionary<string, FileEntry> filesChanged { get; private set; } = new Dictionary<string, FileEntry>();

        /// <summary>
        /// ��Ҫ���ӵ�Ŀ¼�б��������޸�
        /// </summary>
        public static List<string> dirsToWatch= new List<string>() { "Assets" };

        static Dictionary<string, FileSystemWatcher> _fileSystemWatchers = new Dictionary<string, FileSystemWatcher>();
        

        static FileWatcher()
        {
            if(hotReloadEnabled)
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            switch(mode)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    StartWatch();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    StopWatch();
                    break;
                default: break;
            }
        }

        private static void StartWatch()
        {
            HashSet<string> allDirs = new HashSet<string>(dirsToWatch);

            //foreach (string dir in dirsToWatch)
            //{
            //    if (!Directory.Exists(dir))
            //        continue;

            //    // ����Ŀ¼�ķ�������Ҳ�������б�
            //    // (.net6 �ſ�ʼ֧�� DirectoryInfo.LinkTarget���ԣ�����ֻ��win/mac����ʵ��, ����ݲ�����)
            //    string[] subDirs = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories);
            //    foreach(var subDir in subDirs)
            //    {
            //        var di = new DirectoryInfo(subDir);
            //        if(di.Attributes.HasFlag(FileAttributes.ReparsePoint))
            //        {
                        
            //        }
            //    }
            //}

            foreach (string dir in allDirs)
            {
                var watcher = new FileSystemWatcher(dir, "*.cs");
                watcher.IncludeSubdirectories = true;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.EnableRaisingEvents = true;
                watcher.Changed += OnFileChanged;
            }
            
        }

        private static void StopWatch()
        {
            foreach (var watcher in _fileSystemWatchers)
            {
                watcher.Value.Dispose();
            }
            _fileSystemWatchers.Clear();
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            var fullPath = e.FullPath;

            if (!Application.isPlaying) return;
            if(!File.Exists(fullPath)) return;

            if(!filesChanged.TryGetValue(fullPath, out FileEntry entry))
            {
                entry = new FileEntry() { path = fullPath, version = 0, lastModify = DateTime.Now };
                filesChanged.Add(fullPath, entry);
            }
            entry.version++;
            entry.lastModify= DateTime.Now;
            hasChangedSinceLast = true;
        }
    }
}
