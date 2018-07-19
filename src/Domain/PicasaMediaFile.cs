using System.Collections.Generic;

namespace DotnetFer.PicasaParser.Domain
{
    public class PicasaMediaFile
    {
        public PicasaMediaFile()
        {
            Data = new Dictionary<string, string>();
        }
        public Dictionary<string, string> Data { get; set; }

    }
}