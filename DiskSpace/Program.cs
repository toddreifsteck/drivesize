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
            var dirs = new Dirs("COMPUTER");
            string[] drives = Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                Console.WriteLine(drive);
                var subDir = AddSubDirectories(drive, drive);
                if (subDir != null && subDir.size > 0)
                {
                    dirs.directories.Add(subDir);
                    dirs.size = dirs.size + subDir.size;
                }
            }
            Console.WriteLine(watch.Elapsed);

            var json = Jil.JSON.Serialize(dirs);
            File.AppendAllText("C:\\foo.json", json);
        }

        public static Dirs AddSubDirectories(string path, string name)
        {
            Dirs currentDir = null;
            try
            {
                foreach (var direct in new DirectoryInfo(path).EnumerateFileSystemInfos("*"))
                {
                    if (!direct.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        if (direct.Attributes.HasFlag(FileAttributes.Directory))
                        {
                            // Track directory for future addition
                            var subDir = AddSubDirectories(direct.FullName, direct.Name);
                            if (subDir != null && subDir.size > 0)
                            {
                                if (currentDir == null)
                                {
                                    currentDir = new Dirs(name);
                                }

                                currentDir.directories.Add(subDir);
                                currentDir.size = currentDir.size + subDir.size;
                            }
                        }
                        else
                        {
                            if (currentDir == null)
                            {
                                currentDir = new Dirs(name);
                            }

                            // File
                            FileInfo file = (FileInfo)direct;
                            currentDir.files.Add(new FileSize(file.Name, file.Length));
                            currentDir.size = currentDir.size + file.Length;
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

            return currentDir;
        }
    }

    public class Dirs
    {
        public string name;
        public long size;
        public IList<Dirs> directories = new List<Dirs>();
        public IList<FileSize> files = new List<FileSize>();

        public Dirs(string name)
        {
            this.name = name;
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