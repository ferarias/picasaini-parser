using System;
using System.Collections.Generic;

namespace DotnetFer.PicasaParser.Domain
{
    public class PicasaIniData
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description{ get; set; }

        public string Folder { get; set; }

        public Dictionary<string, string> PicasaExtraData { get; set; }
        public Dictionary<string, string> Contacts { get; set; }
        public Dictionary<string, string> Contacts2 { get; set; }
        public Dictionary<string, string> Encoding { get; set; }
        public Dictionary<string, PicasaAlbum> Albums { get; set; }
        public Dictionary<string, PicasaMediaFile> Pictures { get; set; }

        public IList<string> Categories { get; set; }
        public IList<string> P2Categories { get; set; }

        public DateTime Date { get; set; }

        public PicasaIniData()
        {
            Categories = new List<string>();
            P2Categories = new List<string>();
            PicasaExtraData = new Dictionary<string, string>();
            Contacts = new Dictionary<string, string>();
            Contacts2 = new Dictionary<string, string>();
            Encoding = new Dictionary<string, string>();
            Albums = new Dictionary<string, PicasaAlbum>();
            Pictures = new Dictionary<string, PicasaMediaFile>();
        }

        public override string ToString()
        {
            return $"'{Name}' ({Date:D})";
        }
    }
}