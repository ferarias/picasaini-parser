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
            Filters = new List<PicasaFilter>();
            Keywords = new List<string>();
        }

        /// <summary>
        /// keywords assigned in picasa
        /// (only added for non-jpeg photos, stored as IPTC Keywords for jpeg photos)
        /// </summary>
        public IEnumerable<string> Keywords { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public string FileName { get; set; }

        /// <summary>
        /// added if image was starred in picasa
        /// </summary>
        public bool Star { get; set; }

        /// <summary>
        /// caption entered in picasa
        /// (only added for non-jpeg photos, stored as IPTC Caption for jpeg photos)
        /// </summary>
        public string Caption { get; set; }
        public IEnumerable<FaceData> Faces { get; set; }

        public string Rotate { get; set; }

        /// <summary>
        /// all applied filters per photo are recorded to .picasa.ini
        /// to provide an editing history and/or an easier undo facility.
        /// </summary>
        public IEnumerable<PicasaFilter> Filters { get; set; }

        public override string ToString()
        {
            return $"{FileName}. {Faces.Count(),-2} {Filters.Count(),-2} {Data.Count,-2}.";
        }
    }

    public class PicasaFilter
    {
        public PicasaFilter()
        {
            Parameters = new List<string>();
        }
        public string Command { get; set; }
        public IList<string> Parameters { get; set; }
    }
}