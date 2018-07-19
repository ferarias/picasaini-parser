using System.Collections.Generic;

namespace DotnetFer.PicasaParser.Domain
{
    public class PicasaIniData
    {
        public Dictionary<string, string> Picasa { get; set; }
        public Dictionary<string, string> Contacts { get; set; }
        public Dictionary<string, string> Contacts2 { get; set; }
        public Dictionary<string, string> Encoding { get; set; }
        public Dictionary<string, PicasaAlbum> Albums { get; set; }
        public Dictionary<string, PicasaMediaFile> Pictures { get; set; }

        public PicasaIniData()
        {
            Picasa = new Dictionary<string, string>();
            Contacts = new Dictionary<string, string>();
            Contacts2 = new Dictionary<string, string>();
            Encoding = new Dictionary<string, string>();
            Albums = new Dictionary<string, PicasaAlbum>();
            Pictures = new Dictionary<string, PicasaMediaFile>();
        }


    }
}