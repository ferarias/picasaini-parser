using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotnetFer.PicasaParser.Domain;
using McMaster.Extensions.CommandLineUtils;

namespace DotnetFer.PicasaParser.App
{
    [Command(Name = "PicasaParser", Description = "A picasa.ini files parser for a folder structure")]
    [HelpOption("-?")]
    public class Program
    {
        // Entry point
        private static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

        #region "Options"

        [Argument(0, Description = "The root folder")]
        private string RootFolder { get; }

        [Option]
        public (bool HasValue, TraceLevel Level) Verbosity { get; }

        #endregion

        private static TraceLevel _traceLevel;
        private static PicasaIniParser _parser;
        private static IniDataProcessor _processor;

        /// <summary>
        /// Main executing method
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (string.IsNullOrEmpty(RootFolder))
            {
                app.ShowHelp();
                return 0;
            }

            _traceLevel = Verbosity.HasValue ? Verbosity.Level : TraceLevel.Info;

            if (!Directory.Exists(RootFolder))
            {
                Console.Error.WriteLine($"Invalid root folder '{RootFolder}'");
                return 1;
            }
            
            _parser = new PicasaIniParser();
            _processor = new IniDataProcessor();

            LogTrace(TraceLevel.Info, "Picasa folder process starting in " + RootFolder);

            var iniFiles = FindFoldersWithIni(RootFolder);
            LogTrace(TraceLevel.Verbose, $"Found {iniFiles.Count} picasa.ini files");

            var iniData = await ParseIniFilesAsync(iniFiles);
            LogTrace(TraceLevel.Verbose, $"Parsed {iniData.Count} items");

            LogTrace(TraceLevel.Info, "Picasa folder process finished");


            // All categories
            var allCategories = _processor.GetAllCategories(iniData);
            foreach (var category in allCategories)
            {
                Console.WriteLine(category);
            }
            
            // All albums
            var albums = _processor.GetAllAlbums(iniData);
            foreach (var album in albums)
            {
                Console.WriteLine($"{album.Key}:{album.Value}");
            }

            return 0;
        }


        private static async Task<Dictionary<string, PicasaIniData>> ParseIniFilesAsync(IEnumerable<string> iniFiles)
        {
            var dictionary = new Dictionary<string, PicasaIniData>();
            foreach (var file in iniFiles)
            {
                dictionary.Add(file, await _parser.ParseAsync(file));
            }
            return dictionary;
        }

        private static List<string> FindFoldersWithIni(string rootFolder)
        {
            var iniFiles = new List<string>();
            FindInSubtree(rootFolder, iniFiles);
            return iniFiles;
        }

        private static void FindInSubtree(string dir, ICollection<string> data)
        {
            foreach (var folder in Directory.GetDirectories(dir))
            {
                FindInSubtree(folder, data);
            }

            var picasaIniFile = Directory.GetFiles(dir, "picasa.ini").FirstOrDefault();
            if (picasaIniFile != null)
                data.Add(picasaIniFile);
        }

        private static void LogTrace(TraceLevel level, string message)
        {
            if (_traceLevel < level) return;
            switch (level)
            {
                case TraceLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case TraceLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case TraceLevel.Off:
                    break;
                case TraceLevel.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case TraceLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
            
            Console.WriteLine($"{level}: {message}");
            Console.ResetColor();
        }



    }
}
