using System;
using System.Collections.Generic;

namespace DotnetFer.PicasaParser.Domain
{
    public class PicasaAlbum
    {
        public PicasaAlbum()
        {
            PicasaExtraData = new Dictionary<string, string>();
        }
        public string Name { get; set; }
        public DateTimeOffset Date { get; set; }

        public Dictionary<string, string> PicasaExtraData { get; set; }


        public override string ToString()
        {
            return $"'{Name}' ({Date:D})";
        }
    }
}