using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetFer.PicasaParser.Domain;

namespace DotnetFer.PicasaParser
{
    public class PicasaIniParser
    {
        private static List<string> _validExtensions = new List<string> { "JPG", "GIF", "PNG", "MOV", "AVI", "MP4" };
        private const string Pattern = @"\[(.*)\]";
        private static readonly Regex Rgx = new Regex(Pattern, RegexOptions.IgnoreCase);

        private static readonly string PicasaDateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";

        public async Task<PicasaIniData> ParseAsync(string picasaIniFilePath)
        {
            var iniData = new PicasaIniData();
            var folder = Path.GetDirectoryName(picasaIniFilePath);
            iniData.Folder = folder;

            if (!Directory.Exists(folder))
                return iniData;

            var fileStream = new FileStream(picasaIniFilePath, FileMode.Open);
            using (var reader = new StreamReader(fileStream))
            {
                string line;
                var section = "";
                var albumId = "";
                var picture = "";
                var skipSection = false;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var matches = Rgx.Matches(line);
                    if (matches.Count > 0)
                    {
                        section = matches[0].Groups[1].Value.ToUpper();
                        if (section == "PICASA" || section == "CONTACTS" || section == "CONTACTS2" || section == "ENCODING")
                            skipSection = false;

                        else if (section.StartsWith(".ALBUM"))
                        {
                            var idx = section.IndexOf(':');
                            albumId = section.Substring(idx + 1);
                            section = "ALBUM";
                            skipSection = false;
                        }
                        else
                        {
                            var picturePath = Path.Combine(folder, section);
                            //var extension = Path.GetExtension(picturePath);
                            if (Path.HasExtension(picturePath))
                            {
                                if (File.Exists(picturePath))
                                {
                                    picture = picturePath;
                                    section = "PICTURE";
                                    skipSection = false;
                                }
                                else
                                {
                                    Console.WriteLine($"File {picturePath} does not exist!");
                                    skipSection = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"INVALID SECTION {section} in {picasaIniFilePath}");
                                skipSection = true;
                            }
                        }

                    }
                    else
                    {
                        if (skipSection)
                            continue;

                        var keyValueLine = line.Split('=');
                        var keyValuePair = keyValueLine.Length == 2
                            ? new KeyValuePair<string, string>(keyValueLine[0], keyValueLine[1])
                            : new KeyValuePair<string, string>(line, string.Empty);

                        switch (section)
                        {
                            case "":
                            case "PICASA":
                                ParsePicasaSectionItem(iniData, keyValuePair);

                                break;

                            case "CONTACTS":
                                iniData.Contacts.Add(keyValuePair.Key, keyValuePair.Value);
                                break;
                            case "CONTACTS2":
                                iniData.Contacts2.Add(keyValuePair.Key, keyValuePair.Value);
                                break;

                            case "ENCODING":
                                if (iniData.Encoding.ContainsKey(keyValuePair.Key))
                                    iniData.Encoding[keyValuePair.Key] = string.Concat(iniData.Encoding[keyValuePair.Key], ", ", keyValuePair.Value);
                                else
                                    iniData.Encoding.Add(keyValuePair.Key, keyValuePair.Value);
                                break;
                            case "ALBUM":
                                if (!iniData.Albums.ContainsKey(albumId))
                                    iniData.Albums.Add(albumId, new PicasaAlbum());

                                ParsePicasaSectionItem(albumId, iniData.Albums[albumId], keyValuePair);

                                break;
                            case "PICTURE":
                                if (!iniData.Pictures.ContainsKey(picture))
                                    iniData.Pictures.Add(picture, new PicasaMediaFile());
                                if (iniData.Pictures[picture].Data.ContainsKey(keyValuePair.Key))
                                    Console.WriteLine($"Duplicate transform in {picasaIniFilePath} => {picture} => {line}");
                                else
                                    iniData.Pictures[picture].Data.Add(keyValuePair.Key, keyValuePair.Value);
                                break;
                        }
                    }
                }
            }

            return iniData;
        }

        private static void ParsePicasaSectionItem(string albumId, PicasaAlbum picasaAlbum, KeyValuePair<string, string> keyValuePair)
        {
            if (keyValuePair.Key.Equals("token", StringComparison.OrdinalIgnoreCase) &&
                albumId.Equals(keyValuePair.Value, StringComparison.CurrentCultureIgnoreCase)) return;

            if (keyValuePair.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                picasaAlbum.Name = keyValuePair.Value;
            }
            else if (keyValuePair.Key.Equals("date", StringComparison.OrdinalIgnoreCase))
            {
                picasaAlbum.Date = DateTimeOffset.ParseExact(keyValuePair.Value, PicasaDateFormat, CultureInfo.InvariantCulture);
            }
            else
                picasaAlbum.PicasaExtraData.Add(keyValuePair.Key, keyValuePair.Value);
        }

        private static void ParsePicasaSectionItem(PicasaIniData iniData, KeyValuePair<string, string> keyValuePair)
        {
            if (keyValuePair.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
                iniData.Name = keyValuePair.Value;
            else if (keyValuePair.Key.Equals("category", StringComparison.OrdinalIgnoreCase))
                iniData.Categories.Add(keyValuePair.Value);
            else if (keyValuePair.Key.Equals("location", StringComparison.OrdinalIgnoreCase))
                iniData.Location = keyValuePair.Value;
            else if (keyValuePair.Key.Equals("description", StringComparison.OrdinalIgnoreCase))
                iniData.Description = keyValuePair.Value;

            else if (keyValuePair.Key.Equals("P2category", StringComparison.OrdinalIgnoreCase))
                iniData.P2Categories.Add(keyValuePair.Value);
            else if (keyValuePair.Key.Equals("date", StringComparison.OrdinalIgnoreCase))
                iniData.Date = PicasaDateParser.PicasaDateToDateTime(keyValuePair.Value);

            else if (iniData.PicasaExtraData.ContainsKey(keyValuePair.Key))
                iniData.PicasaExtraData[keyValuePair.Key] = string.Concat(iniData.PicasaExtraData[keyValuePair.Key], ", ", keyValuePair.Value);
            else
                iniData.PicasaExtraData.Add(keyValuePair.Key, keyValuePair.Value);
        }


    }
}
