using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace FileSorterOptions
{
    class Mover
    {

        private const string source = "C:\\Users\\Lukas\\Downloads";
        private string[] allFiles = null;
        private Dictionary<string, string> destinations;

        //Dictionary, <extenstion, destinationFolder>
        public Mover(Dictionary<string, string> locations) {
            allFiles = Directory.GetFiles(source);
            destinations = locations;
        }

        public void Move() {
            foreach (string filePath in allFiles) {
                string fileExt = Path.GetExtension(filePath);
                foreach (string ext in destinations.Keys) {
                    if(ext == fileExt && !fileLocked(filePath)) {
                        try
                        {
                            File.Move(filePath, destinations[ext] + "\\"+ Path.GetFileName(filePath));
                        }
                        catch (Exception e) {
                            Console.WriteLine("Exception: " + e.ToString());
                        }
                    }
                }

            }
        }


        private bool fileLocked(string path)
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
