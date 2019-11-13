﻿using Settlement.Classes.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Classes.FileMonitor
{
    class FileWatcher
    {
        private FileSystemWatcher watcher;
        public static List<string> newFile;

        public FileWatcher()
        {
            string settlementDir = TKHelper.GetDirectoryName() + @"\settlement";
            watcher = new FileSystemWatcher(@settlementDir);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.Created += watcher_Created;
            watcher.Renamed += wathcer_Renamed;
            watcher.Deleted += watcher_Deleted;
            newFile = new List<string>();
        }

        static void wathcer_Renamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("File : {0} renamed to {1} at time : {2}", e.OldName, e.Name, DateTime.Now.ToLocalTime());
        }

        static void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File : {0} deleted at time : {1}", e.Name, DateTime.Now.ToLocalTime());
        }

        static void watcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File : {0} created at time : {1}", e.Name, DateTime.Now.ToLocalTime());
            newFile.Add(e.Name);
        }
    }
}
