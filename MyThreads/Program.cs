using System;
using System.IO;


namespace MyThreads
{
    class Program
    {
        public static int copiedFilesCount = 0;
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter source directory:");
            string sourceDirectoryName = Console.ReadLine();
            Console.WriteLine("Please enter destination directory:");
            string destinationDirectoryName = Console.ReadLine();

            if (sourceDirectoryName.Equals(destinationDirectoryName))
                throw new ArgumentException("Source and destination directories cant be equal");
            if (!Directory.Exists(sourceDirectoryName))
                throw new ArgumentException("Wrong source directory");
            if (!Directory.Exists(destinationDirectoryName))
            {
                try
                {
                    Directory.CreateDirectory(destinationDirectoryName);
                }
                catch
                {
                    throw new Exception("Cant create and find destination directory");
                }
            }

            TaskQueue taskQueue = new TaskQueue(100);

            foreach (var directory in Directory.GetDirectories(sourceDirectoryName, "*", SearchOption.AllDirectories))
            {
                string newDirectory = directory.Replace(sourceDirectoryName, destinationDirectoryName);
                try
                {
                    Directory.CreateDirectory(newDirectory);
                }
                catch 
                {
                    Console.WriteLine("Error to create directory");
                }
            }
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            foreach (var file in Directory.GetFiles(sourceDirectoryName, "*.*", SearchOption.AllDirectories))
            {
                string newFile = file.Replace(sourceDirectoryName, destinationDirectoryName);
                taskQueue.EnqueueTask(delegate {
                    try
                    {
                        File.Copy(file, newFile, true);
                        copiedFilesCount++;
                    }
                    catch
                    {
                        Console.WriteLine("Error occured while copying files");
                    }
                });
            }
            
            taskQueue.Interrupt();
            Console.WriteLine("Files copied = " + copiedFilesCount);
            sw.Stop();
            Console.WriteLine("Time: " + sw.ElapsedMilliseconds + " ms");
            Console.ReadLine();
        }

    }
}
