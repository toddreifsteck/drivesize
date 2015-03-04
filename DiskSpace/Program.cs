using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskSpace
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = Stopwatch.StartNew();
            var dirs = new Dirs("COMPUTER", null);
            string[] drives = Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                Console.WriteLine(drive);
                var subDir = AddSubDirectories(drive, drive, dirs);
                if (subDir != null && subDir.totalSize > 0)
                {
                    dirs.directories.Add(subDir);
                    dirs.totalSize = dirs.totalSize + subDir.totalSize;
                }
            }
            Console.WriteLine(watch.Elapsed);

            var json = Jil.JSON.Serialize(dirs);
            File.AppendAllText("C:\\foo.json", json);
            Console.ReadLine();
        }

        public static Dirs AddSubDirectories(string path, string name, Dirs dirs)
        {
            Dirs currentDir = new Dirs(name, path);
            try
            {
                FileInfo file;
                foreach (var direct in new DirectoryInfo(path).EnumerateFileSystemInfos("*"))
                {
                    //direct.Attributes.HasFlag(FileAttributes.Directory)
                    file = direct as FileInfo;
                    if (file == null)
                    {
                        // Track directory for future addition
                        var subDir = AddSubDirectories(direct.FullName, direct.Name, currentDir);
                        if (subDir != null && subDir.totalSize > 0)
                        {
                            currentDir.directories.Add(subDir);
                            currentDir.totalSize = currentDir.totalSize + subDir.totalSize;
                        }
                    }
                    else
                    {
                        // File
                        currentDir.files.Add(new FileSize(file.Name, file.Length));
                        currentDir.totalSize = currentDir.totalSize + file.Length;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Unauthorized: " + name);
            }
            catch (IOException e)
            {
                Console.WriteLine("Error on drive: " + name);
                Console.WriteLine(e);
            }

            if (currentDir.totalSize > 0)
            {
                return currentDir;
            }
            else
            {
                return null;
            }
        }
    }

    public class Dirs
    {
        public string name;
        public string path;
        public long totalSize;
        public IList<Dirs> directories = new List<Dirs>();
        public IList<FileSize> files = new List<FileSize>();   
     
        public Dirs(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }

    public class FileSize
    {
        public string name;
        public long size;
        
        public FileSize(string name, long size)
        {
            this.name = name;
            this.size = size;
        }
    }
}