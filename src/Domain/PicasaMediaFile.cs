using System.Collections.Generic;
using System.Linq;

namespace DotnetFer.PicasaParser.Domain
{
    public class PicasaMediaFile
    {
        public PicasaMediaFile()
        {
            Data = new Dictionary<string, string>();
            Faces = new List<FaceData>();
            Filters = new List<string>();
        }
        public Dictionary<string, string> Data { get; set; }

        public string FileName { get; set; }
        public IEnumerable<FaceData> Faces { get; set; }

        public string Rotate { get; set; }
        public IEnumerable<string> Filters { get; set; }

        public override string ToString()
        {
            return $"{FileName}. {Faces.Count(),-2} {Filters.Count(),-2} {Data.Count,-2}.";
        }
    }
}