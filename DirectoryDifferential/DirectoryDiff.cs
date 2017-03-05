using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace QueryCompareTwoDirs
{
    class CompareDirs
    {
        static void Main(string[] args)
        {
            // Create two identical or different temporary folders on a local drive and change these file paths. 
            string OldDirectory = "d";
            string NewDirectory = "d";
            string LogFileDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            WriteToLog("Directory Differential v1.0 executed", LogFileDate + "-DirectoryDiff.log");
            if (args.Length == 0 || ((args[0] == "/?" | args[1] == "-?") & args.Length == 1))
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("DirectoryDiff v1.0 by SONNY");
                Console.WriteLine("Compares contents of New Directory against an Old Directory");
                Console.WriteLine();
                Console.WriteLine("Next time, speed this up by passing arguments like this...");
                Console.WriteLine("usage:   directorydiff.exe <New Directory> <Old Directory>");
            }
            if (args.Length != 2 || !Directory.Exists(".\\" + args[0]) | !Directory.Exists(".\\" + args[1]))
            {
                Console.WriteLine("Only 2 relative path directories can be compared...");
                while (!Directory.Exists(".\\" + OldDirectory))
                {
                    Console.Write("Old directory:");
                    OldDirectory = Console.ReadLine();
                    if (!Directory.Exists(".\\" + OldDirectory))
                    {
                        Console.WriteLine(OldDirectory + " does not exist in relative path");
                    }

                }
                while (!Directory.Exists(".\\" + NewDirectory))
                {
                    Console.Write("New directory:");
                    NewDirectory = Console.ReadLine();
                    if (!Directory.Exists(".\\" + NewDirectory))
                    {
                        Console.WriteLine(NewDirectory + " does not exist in relative path");
                    }
                }
            }
            else
            {
                OldDirectory = args[0];
                NewDirectory = args[1];
            }
            WriteToLog("Old Directory: " + OldDirectory + " subdirectory exists", LogFileDate + "-DirectoryDiff.log");
            WriteToLog("New Directory: " + NewDirectory + " subdirectory exists", LogFileDate + "-DirectoryDiff.log");

            //I inverted the dir below
            System.IO.DirectoryInfo dir2 = new System.IO.DirectoryInfo(OldDirectory);
            System.IO.DirectoryInfo dir1 = new System.IO.DirectoryInfo(NewDirectory);

            // Take a snapshot of the file system.
            IEnumerable<System.IO.FileInfo> list1 = dir1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            IEnumerable<System.IO.FileInfo> list2 = dir2.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            //A custom file comparer defined below
            FileCompare myFileCompare = new FileCompare();

            // This query determines whether the two folders contain identical file lists, based on the custom file comparer 
            // that is defined in the FileCompare class. The query executes immediately because it returns a bool. 
            bool areIdentical = list1.SequenceEqual(list2, myFileCompare);

            if (areIdentical == true)
            {
                Console.WriteLine("The two folders have the same directory list");
                WriteToLog("The two folders have the same directory list", LogFileDate + "-DirectoryDiff.log");
            }
            else
            {
                Console.WriteLine("The two folders DO NOT have the same directory list");
                WriteToLog("The two folders DO NOT have the same directory list", LogFileDate + "-DirectoryDiff.log");
            }

            // Find the common files. It produces a sequence and doesn't execute until the foreach statement. 
            var queryCommonFiles = list1.Intersect(list2, myFileCompare);

            if (queryCommonFiles.Count() > 0)
            {
                Console.WriteLine("The following files are in both folders:");
                foreach (var v in queryCommonFiles)
                {
                    Console.WriteLine(v.FullName); //shows which items end up in result list
                }
            }
            else
            {
                Console.WriteLine("There are no common files in the two folders.");
                WriteToLog("No differences between the directories were found", LogFileDate + "-DirectoryDiff.log");
            }
            //Creates DELTA subdirectory if needed to copy original file
            string deltadir = string.Concat(".\\", NewDirectory, "-DELTA\\");
            if (!Directory.Exists(deltadir))
            {
                Directory.CreateDirectory(deltadir);
                //New folder does not have same metadata as original. Line below was (failed) attempt at copying permissions
                //Directory.SetAccessControl(deltadir, Directory.GetAccessControl(".\\" + NewDirectory));
            }
            // Identify NewDirectory/files in NewDirectory that's different from path OldDirectory/files
            var queryList1Only = (from file in list1 select file).Except(list2, myFileCompare);

            Console.WriteLine("The following files are in NewDirectory but not OldDirectory:");
            WriteToLog("These files are different and were copied to the DELTA folder:", LogFileDate + "-DirectoryDiff.log");
            //When NewDirectory/file is different, copy file to NewDirectory-DELTA
            foreach (var v in queryList1Only)
            {
                Console.WriteLine(v.FullName);
                WriteToLog(v.FullName, LogFileDate + "-DirectoryDiff.log");
                string newdelta = ReplaceFirst(v.DirectoryName, NewDirectory, NewDirectory + "-DELTA");
                if (!Directory.Exists(newdelta))
                {
                    Directory.CreateDirectory(newdelta);
                    //New folder does not have same metadata as original. Line below was (failed) attempt at copying permissions
                    //Directory.SetAccessControl(newdelta, Directory.GetAccessControl(".\\" + v.Directory));
                }
                v.CopyTo(newdelta + "\\" + v);
            }

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
        //function substitutes ONLY the first instance of string 'search' in string 'text' with string 'replace'
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        public static void WriteToLog(string text, string logfile)
        {
            using (StreamWriter w = File.AppendText(logfile))
            {
                Log(text, w);
            }
        }
        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine("{0}:: {1}", DateTime.Now.ToString(), logMessage);
        }
    }

    // This implementation defines a very simple comparison between two FileInfo objects. It only compares the name 
    // of the files being compared and their length in bytes. 
    class FileCompare : System.Collections.Generic.IEqualityComparer<System.IO.FileInfo>
    {
        public FileCompare() { }

        public bool Equals(System.IO.FileInfo f1, System.IO.FileInfo f2)
        {
            return (f1.Name == f2.Name && f1.Length == f2.Length);
        }

        // Return a hash that reflects the comparison criteria. According to the rules for IEqualityComparer<T>, 
        //if Equals is true, then the hash codes must also be equal. Because equality as defined here is a simple
        //value equality, not reference identity, it is possible that two or more objects will produce the same hash code. 
        public int GetHashCode(System.IO.FileInfo fi)
        {
            string s = String.Format("{0}{1}", fi.Name, fi.Length);
            return s.GetHashCode();
        }
    }
}