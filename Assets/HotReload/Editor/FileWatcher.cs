using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptHotReload
{
    /// <summary>
    /// ����cs�ļ��仯
    /// </summary>
    [InitializeOnLoad]
    public class FileWatcher
    {
        class FileEntry
        {
            public string name;
            public int version;
            public DateTime lastModify;
        }

        /// <summary>
        /// ��Ҫ���ӵ�Ŀ¼�б��������޸�
        /// </summary>
        /// <remarks>ע�⣺�Է���������Ч�����Կ��ǵݹ鴴���������ӵ�watcher</remarks>
        static List<string> dirsToWatch= new List<string>() { "Assets" };

        static FileWatcher()
        {

        }

    }
}
