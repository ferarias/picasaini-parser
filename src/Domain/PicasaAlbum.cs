using System.Collections.Generic;

namespace DotnetFer.PicasaParser.Domain
{
    public class PicasaAlbum
    {
        public PicasaAlbum()
        {
            Data = new Dictionary<string, string>();
        }
        public Dictionary<string, string> Data { get; set; }

    }
}