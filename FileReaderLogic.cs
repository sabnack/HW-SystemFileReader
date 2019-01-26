using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SystemFileReader
{
    internal class FileReaderLogic
    {
        public volatile List<string> FolderList;
        public volatile List<string> FileList;
        //  public List<string> Log;
        private static object _locker = new object();
        private DirectoryInfo RootDir;
        private FileInfo[] Files;
        private DirectoryInfo[] SubDirs;
        private string TreePath;
        private string LogPath;
        public EventWaitHandle WaitHandler = new ManualResetEvent(true);

        public FileReaderLogic(string path, string treePath, string logPath)
        {
            RootDir = Directory.GetParent(path);
            FolderList = new List<string>();
            FileList = new List<string>();
            //  Log = new List<string>();
            TreePath = treePath;
            LogPath = logPath;
        }

        public void Start()
        {
            WalkDirectoryTree(RootDir);
        }

        private void WalkDirectoryTree(DirectoryInfo root)
        {

            try
            {
                Files = root.GetFiles("*.*");
                SubDirs = root.GetDirectories();
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Add(e.Message);
                using (var sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(e.Message);
                }
            }

            catch (DirectoryNotFoundException e)
            {
                //Log.Add(e.Message);
                using (var sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(e.Message);
                }
            }

            if (Files == null) return;
            foreach (var fi in Files)
            {
                WaitHandler.WaitOne();
                lock (_locker)
                {
                    if (FileList.Any(x => x.Contains(fi.FullName)))
                    {
                        continue;
                    }
                    FileList.Add($"{fi.FullName} TaskID {Task.CurrentId}");
                    Console.WriteLine($"{fi.FullName} TaskID {Task.CurrentId}");
                }
            }

            foreach (var dirInfo in SubDirs)
            {
                lock (_locker)
                {
                    if (!FolderList.Any(x => x.Contains(dirInfo.FullName)))
                    {
                        FolderList.Add($"{dirInfo.FullName}  TaskID {Task.CurrentId}");
                        Console.WriteLine($"{dirInfo.FullName}  TaskID {Task.CurrentId}");
                    }
                }
                WaitHandler.WaitOne();
                WalkDirectoryTree(dirInfo);
            }
        }
    }
}
