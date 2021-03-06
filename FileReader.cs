﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SystemFileReader
{
    public class FileReader
    {
        private string ViewPath { get; }
        private readonly string _treePath = @"D:\tmp\folderTree.txt";
        private readonly string _logPath = @"D:\tmp\log.txt";
        private ConsoleKeyInfo _key;
        private Task[] _tasks;
        private static readonly CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _token = CancelTokenSource.Token;

        public FileReader(string viewPath = @"d:\onedrive\")
        {
            ViewPath = viewPath;
        }

        public void Start()
        {
            if (!Directory.GetParent(ViewPath).Exists)
            {
                Console.WriteLine($"Folder dosen`t exist");
                return;
            }

            if (!Directory.GetParent(_treePath).Exists)
            {
                Directory.CreateDirectory(Directory.GetParent(_treePath).ToString());
            }

            if (File.Exists(_treePath))
            {
                File.Delete(_treePath);
            }

            var root = new FileReaderLogic(ViewPath, _treePath, _logPath);

            _tasks = new Task[3] { new Task(() => root.Start(_token)), new Task(() => root.Start(_token)), new Task(() => root.Start(_token)) };

            foreach (var task in _tasks)
            {
                task.Start();
            }

            while (true)
            {
                if (_tasks.All(x => x.Status == TaskStatus.RanToCompletion)) break;

                if (_key.Key == ConsoleKey.F1)
                    root.WaitHandler.Reset();
                else if (_key.Key == ConsoleKey.F2)
                    root.WaitHandler.Set();
                else if (_key.Key == ConsoleKey.F3)
                    foreach (var task in _tasks)
                    {
                        Console.WriteLine($"TaskId {task.Id} {task.Status}");
                    }
                else if (_key.Key == ConsoleKey.Escape)
                {
                    CancelTokenSource.Cancel();
                }
                _key = Console.ReadKey(true);
            }

            Console.WriteLine(new string('-',30));
            File.WriteAllLines(_treePath, root.FolderList);
            File.AppendAllLines(_treePath, root.FileList);

            Console.WriteLine("Done");
        }
    }
}
