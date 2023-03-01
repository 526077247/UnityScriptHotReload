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

        /// <summary>
        /// ��Ҫ���ӵ�Ŀ¼�б��������޸�
        /// </summary>
        public static List<string> dirsToWatch = new List<string>() { "Assets" };

        public static bool changedSinceLastGet { get; private set; } = false;
        public static DateTime lastModifyTime { get; private set; } = DateTime.MinValue;

        static Dictionary<string, FileEntry> _filesChanged = new Dictionary<string, FileEntry>();
        static Dictionary<string, FileSystemWatcher> _fileSystemWatchers = new Dictionary<string, FileSystemWatcher>();
        static bool _isWatching = false;
        static object _locker = new object();

        static FileWatcher()
        {
            if(hotReloadEnabled)
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static string[] GetChangedFile()
        {
            var ret = new List<string>();
            lock(_locker)
            {
                ret.AddRange(_filesChanged.Keys);
                changedSinceLastGet = false;
            }
            return ret.ToArray();
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
                _fileSystemWatchers.Add(dir, watcher);
            }
            _isWatching= true;
        }

        private static void StopWatch()
        {
            _isWatching= false;
            foreach (var watcher in _fileSystemWatchers)
            {
                watcher.Value.Dispose();
            }
            _fileSystemWatchers.Clear();
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!_isWatching) return; // �˻ص������������߳�ִ�У���˲���ʹ�� Application.isPlaying �ж�

            var fullPath = e.FullPath.Replace('\\', '/');
            if (fullPath.Contains("/HotReload/Editor/")) return; // �������·������reload

            if(!File.Exists(fullPath)) return;

            var now = DateTime.Now;
            lock(_locker)
            {
                if (!_filesChanged.TryGetValue(fullPath, out FileEntry entry))
                {
                    entry = new FileEntry() { path = fullPath, version = 0, lastModify = now };
                    _filesChanged.Add(fullPath, entry);
                }
                entry.version++;
                entry.lastModify = now;
                changedSinceLastGet = true;
                lastModifyTime = now;
            }
        }
    }
}
