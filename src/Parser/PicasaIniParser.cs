using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DotnetFer.Picasa.Parser
{
    public class PicasaIniParser
    {
        private static List<string> validExtensions = new List<string> {"JPG", "GIF", "PNG", "MOV", "AVI", "MP4"};
        const string pattern = @"\[(.*)\]";
        static Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
        public PicasaIniData Parse(string iniFilePath)
        {
            var iniData = new PicasaIniData();
            var folder = Path.GetDirectoryName(iniFilePath);
            var data = new StringWriter();

            if (!Directory.Exists(folder))
                return iniData;

            var fileStream = new FileStream(iniFilePath, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line;
                string section = "";
                string albumId = "";
                string picture = "";
                bool skipSection = false;
                while ((line = reader.ReadLine()) != null)
                {
                    var matches = rgx.Matches(line);
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
                            if(Path.HasExtension(picturePath))
                            {
                                if (File.Exists(picturePath))
                                {
                                    picture = picturePath;
                                    section = "PICTURE";
                                    skipSection = false;
                                }
                                else
                                {
                                    System.Console.WriteLine($"File {picturePath} does not exist!");
                                    skipSection = true;
                                }
                            }
                            else
                                {
                                    Console.WriteLine($"INVALID SECTION {section}");
                                    skipSection = true;
                                }
                        }
                        
                    }
                    else
                    {
                        if(skipSection)
                            continue;

                        string k, v;
                        var d = line.Split('=');
                        if (d.Length == 2)
                        {
                            k = d[0]; v = d[1];
                        }
                        else
                        {
                            k = line; v = null;
                        }

                        switch (section)
                        {
                            case "":
                            case "PICASA":
                                if (iniData.Picasa.ContainsKey(k))
                                    iniData.Picasa[k] = string.Concat(iniData.Picasa[k], ", ", v);
                                else
                                    iniData.Picasa.Add(k, v);
                                break;

                            case "CONTACTS":
                                iniData.Contacts.Add(k, v);
                                break;
                            case "CONTACTS2":
                                iniData.Contacts2.Add(k, v);
                                break;

                            case "ENCODING":
                                if (iniData.Encoding.ContainsKey(k))
                                    iniData.Encoding[k] = string.Concat(iniData.Encoding[k], ", ", v);
                                else
                                    iniData.Encoding.Add(k, v);
                                break;
                            case "ALBUM":
                                if (!iniData.Albums.ContainsKey(albumId))
                                    iniData.Albums.Add(albumId, new Album());
                                iniData.Albums[albumId].Data.Add(k, v);
                                break;
                            case "PICTURE":
                                if (!iniData.Pictures.ContainsKey(picture))
                                    iniData.Pictures.Add(picture, new Picture());
                                if(iniData.Pictures[picture].Data.ContainsKey(k))
                                    System.Console.WriteLine($"Duplicate transform in {iniFilePath} => {picture} => {line}");
                                else
                                    iniData.Pictures[picture].Data.Add(k, v);
                                break;
                        }
                    }
                }
            }

            return iniData;
        }
    }

    public class PicasaIniData
    {
        public Dictionary<string, string> Picasa { get; set; }
        public Dictionary<string, string> Contacts { get; set; }
        public Dictionary<string, string> Contacts2 { get; set; }
        public Dictionary<string, string> Encoding { get; set; }
        public Dictionary<string, Album> Albums { get; set; }
        public Dictionary<string, Picture> Pictures { get; set; }

        public PicasaIniData()
        {
            Picasa = new Dictionary<string, string>();
            Contacts = new Dictionary<string, string>();
            Contacts2 = new Dictionary<string, string>();
            Encoding = new Dictionary<string, string>();
            Albums = new Dictionary<string, Album>();
            Pictures = new Dictionary<string, Picture>();
        }


    }
    public class Album
    {
        public Album()
        {
            Data = new Dictionary<string, string>();
        }
        public Dictionary<string, string> Data { get; set; }

    }

    public class Picture
    {
        public Picture()
        {
            Data = new Dictionary<string, string>();
        }
        public Dictionary<string, string> Data { get; set; }

    }
}
