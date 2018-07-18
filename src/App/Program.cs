using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using DotnetFer.Picasa.Parser;

namespace DotnetFer.PicasaParser.App
{
    public class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Picasa folder process");

            var result = Parser.Default.ParseArguments<Options>(args);
            var exitCode = result.MapResult(ProgramMain, errors =>
                                            {
                                                Console.WriteLine(errors);
                                                return 1;
                                            });
            Console.WriteLine("Picasa folder process finished with exit code " + exitCode);
            return exitCode;

        }

        private static int ProgramMain(Options options)
        {
            if (options.Verbose) 
                Console.WriteLine("Starting folder: {0}", options.StartingFolder);

            if (!Directory.Exists(options.StartingFolder))
            {
                Console.WriteLine("Folder '{0}' does not exist.", options.StartingFolder);
                return 1;
            }
            else
            {
                Console.WriteLine("Starting in '{0}'.", options.StartingFolder);
            }

            var iniFiles = new List<string>();
            var iniData = new Dictionary<string, PicasaIniData>();

            ProcessFolder(options.StartingFolder, iniFiles);

            var parser = new PicasaIniParser();
            foreach(var picasaIniFile in iniFiles)
            {
                iniData.Add(picasaIniFile, parser.Parse(picasaIniFile));
            }

            return 0;
        }

        private static void ProcessFolder(string dir, List<string> data)
        {
            foreach (var folder in Directory.GetDirectories(dir))
            {
                ProcessFolder(folder, data);
            }

            var picasaIniFile = Directory.GetFiles(dir, "picasa.ini").FirstOrDefault();
            if (picasaIniFile != null)
                data.Add(picasaIniFile);
        }


    }
}
