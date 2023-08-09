using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter directory:");
            string path = Console.ReadLine();
            if(path == "")
            {
                Console.WriteLine("Path is missing");
                return;
            }

            if (Directory.Exists(path))
            {
                foreach (string file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    FileAttributes atributes = File.GetAttributes(file);
                    if ((atributes & FileAttributes.Hidden) == FileAttributes.Hidden && (atributes & FileAttributes.System) == FileAttributes.System)
                    {
                        Console.WriteLine(file);
                    }
                }
            }
            else if (!Directory.Exists(path))
            {
                Console.WriteLine("Error");
            }

        }


    }
}
