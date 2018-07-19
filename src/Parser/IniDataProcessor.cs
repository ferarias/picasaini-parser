using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotnetFer.PicasaParser.Domain;

namespace DotnetFer.PicasaParser
{
    public class IniDataProcessor
    {
        public IniDataProcessor()
        {
        }

        public IEnumerable<string> GetAllCategories(Dictionary<string, PicasaIniData> iniData)
        {
            return iniData.Values
                .SelectMany(i => i.Picasa)
                .Where(j => j.Key.Equals("P2Category", StringComparison.CurrentCultureIgnoreCase))
                .Select(k => k.Value);
        }

        public Dictionary<string, string> GetAllAlbums(Dictionary<string, PicasaIniData> iniData)
        {
            var albums = new Dictionary<string, string>();
            foreach (var value in iniData.Values)
            foreach (var album in value.Albums)
            foreach (var data in album.Value.Data)
            {
                if (data.Key != "name") continue;
                if(!albums.ContainsKey(album.Key))
                    albums.Add(album.Key, data.Value);

            }

            return albums;
        }

    }
}
