using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;

namespace FileSorterOptions
{
    class Mover
    {

        private const string source = @"C:\\Users\\Lukas\\Downloads";
        private string[] allFiles = null;
        private Dictionary<string, string> destinations;

        //Dictionary, <extenstion, destinationFolder>
        public Mover(Dictionary<string, string> locations, SearchOption searchOption) {
            allFiles = Directory.GetFiles(source, "*", searchOption);
            destinations = locations;
        }

        public void Move( Progress<int> progress) {
            if (allFiles.Length == 0)
            {
                ((IProgress<int>)progress).Report(100);
                return;
            }
            int j = (100 / allFiles.Length);
            int i = 0;
            foreach (string filePath in allFiles) {
                string fileExt = Path.GetExtension(filePath);
                foreach (string ext in destinations.Keys) {
                    if(ext == fileExt && !FileLocked(filePath)) {
                        try
                        {
                            File.Move(filePath, destinations[ext] + "\\"+ Path.GetFileName(filePath));
                        }
                        catch (Exception e) {
                            Console.WriteLine("Exception: " + e.ToString());
                        }
                    }
                    i += j;
                    ((IProgress<int>)progress).Report(i);
                }
            }
            i += j;
            ((IProgress<int>)progress).Report(i);
        }


        private bool FileLocked(string path)
        {
            FileStream file = null;
            try
            {
                file = File.OpenRead(path);
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if(file != null)
                    file.Close();
            }
            return false;

        }

    }
}
