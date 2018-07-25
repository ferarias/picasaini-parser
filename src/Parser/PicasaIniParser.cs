using System;
using System.Collections.Generic;
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
                                    iniData.Pictures.Add(picture, new PicasaMediaFile { FileName = Path.GetFileName(picture) });

                                ParsePicasaSectionItem(iniData.Pictures[picture], keyValuePair);

                                break;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(iniData.Name))
                iniData.Name = new DirectoryInfo(Path.GetDirectoryName(picasaIniFilePath)).Name;

            return iniData;
        }

        private static void ParsePicasaSectionItem(PicasaMediaFile picasaMediaFile, KeyValuePair<string, string> keyValuePair)
        {
            if (keyValuePair.Key.StartsWith("BKTag") || keyValuePair.Key.Equals("backuphash"))
                return;

            if (keyValuePair.Key.Equals("star", StringComparison.OrdinalIgnoreCase))
                picasaMediaFile.Star = keyValuePair.Value == "yes";
            else if (keyValuePair.Key.Equals("caption", StringComparison.OrdinalIgnoreCase))
                picasaMediaFile.Caption = keyValuePair.Value;
            else if (keyValuePair.Key.Equals("keywords", StringComparison.OrdinalIgnoreCase))
                picasaMediaFile.Keywords = keyValuePair.Value.Split('.');
            else if (keyValuePair.Key.Equals("faces", StringComparison.OrdinalIgnoreCase))
                picasaMediaFile.Faces = ParseFaces(keyValuePair.Value);
            else if (keyValuePair.Key.Equals("rotate", StringComparison.OrdinalIgnoreCase))
                picasaMediaFile.Rotate = keyValuePair.Value;
            else if (keyValuePair.Key.Equals("filters", StringComparison.OrdinalIgnoreCase))
                picasaMediaFile.Filters = ParseFilters(keyValuePair.Value);
            else if (picasaMediaFile.Data.ContainsKey(keyValuePair.Key))
                Console.WriteLine($"Duplicate picasa media data in {picasaMediaFile}");
            else
                picasaMediaFile.Data.Add(keyValuePair.Key, keyValuePair.Value);
        }

        /// <summary>
        /// Example: rect64(4ae03855654062d5),25d7d67eaa5784c9;rect64(aeff562ac1df7455),99554ac4bd8f1a90
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static IEnumerable<FaceData> ParseFaces(string value)
        {
            var faceDatas = new List<FaceData>();
            var faces = value.Split(';');
            foreach (var face in faces)
            {
                var faceContact = face.Split(',');
                if (faceContact.Length != 2) continue;
                faceDatas.Add(new FaceData
                {
                    Rectangle = PicasaRectParser.Parse(faceContact[0]),
                    Contact = faceContact[1]
                });
            }

            return faceDatas;
        }

        /// <summary>
        /// Basic filter key format:
        /// the filters key of each photo stores a semicolon-separated list of filter entries:
        ///  filters=enhance=1;crop64=1,45930000ba03defe;
        ///
        /// each entry follows the format:
        ///  &lt;filter identifier&gt;=1,&lt;filter value 1&gt;,&lt;filter value 2&gt;,&lt;..filter value n&gt;;
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// # Here is a list of valid filter identifiers
        /// #
        /// #|--Identifier-|--------------Parameters-------------|----------Description-----------|---------Example---------------|
        /// #| crop64      |  CROP_RECTANGLE*                    |   crop filter, crops the image | crop64=1,30a730d2bf1ab897     |
        /// #|             |                                     |    according to crop rectangle |                               |
        /// #| tilt        | !TILT_ANGLE,!SCALE                  |  tilts and scales image        | tilt=1,0.280632,0.000000      |
        /// #| redeye      |                                     |  redeye removal                | redeye=1                      |
        /// #| enhance     |                                     | "I'm feeling lucky" enhancement| enhance=1                     |
        /// #| autolight   |                                     | automatic contrast correction  | autolight=1                   |
        /// #| autocolor   |                                     | automatic color correction     | autocolor=1                   |
        /// #| retouch     |                                     | retouch                        | retouch=1                     |
        /// #| finetune2   | (unidentified params)               | finetuning (brightness,        | finetune2=1,0.000000,0.000000,|
        /// #|             |                                     |highlights, shadows,color temp) | 0.000000,fff7f5f3,0.000000;   |
        /// #| unsharp2    | !AMOUNT                             | unsharp mask filter            | unsharp2=1,0.600000;          |
        /// #| sepia       |                                     | sepia filter (no params)       | sepia=1                       |
        /// #| bw          |                                     | black/white filter (no params) | bw=1                          |
        /// #| warm        |                                     | warming filter (no params)     | bw=1                          |
        /// #| grain2      |                                     | film grain filter (no params)  | grain2=1                      |
        /// #| tint        |!!PRESERVE_COLOR ,#TINT COLOR        | tint filter                    | tint=1,79.842102,ffff         |
        /// #| sat         |!SATURATION                          | saturation filter              | sat=1,0.161800;               |
        /// #| radblur     |!MOUSE_X,!MOUSE_Y,!SIZE,!AMOUNT      | radial blur                    | radblur=1,0.500000,0.500000,  |
        /// #|             |                                     |                                | 0.239766,0.146199;            | 
        /// #| glow2       |!INTENSITY,!!RADIUS                  | glow effect                    | glow2=1,0.650000,3.000000;    |
        /// #| ansel       |#COLOR                               | filtered black/white           | ansel=1,ffffffff;             |
        /// #| radsat      |!MOUSE_X,!MOUSE_Y,!RADIUS,!SHARPNESS | radial saturation              | radsat=1,0.421652,0.594697,   |
        /// #|             |                                     |                                | 0.333333,0.309942;            | 
        /// #| dir_tint    |!MOUSE_X,!MOUSE_Y,!GRADIENT,!SHADOW  | directed gradient              | dir_tint=1,0.306743,0.401515, |
        /// #|             |                                     |                                | 0.250000,0.250000,ff5bfff3;   |
        /// # 
        /// # LEGEND: 
        /// # ! = float between 0 and 1, precision:6
        /// # !! = float with arbitrary range, precision:6
        /// # # = 32-bit color in hex notation, e.g.: fff7f5f3
        /// # [] = crop rectangle
        /// </remarks>
        /// <returns></returns>
        private static IEnumerable<PicasaFilter> ParseFilters(string value)
        {
            var picasaFilters = new List<PicasaFilter>();

            var filterEntries = value.Split(';');
            foreach (var filterEntry in filterEntries)
            {
                var picasaFilter = new PicasaFilter();
                var entryParts = filterEntry.Split(',');
                var len = entryParts.Length;
                if (len < 1) return picasaFilters;

                picasaFilter.Command = entryParts[0];

                picasaFilter.Parameters = new List<string>(len - 1);
                for (var i = 1; i < len - 1; i++)
                {
                    picasaFilter.Parameters.Add(entryParts[i]);
                }

                picasaFilters.Add(picasaFilter);
                
            }
            return picasaFilters;
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
