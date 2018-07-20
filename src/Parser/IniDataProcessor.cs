using System;
using System.Collections.Generic;
using System.Linq;
using DotnetFer.PicasaParser.Domain;

namespace DotnetFer.PicasaParser
{
    public class IniDataProcessor
    {
        public IniDataProcessor()
        {
        }

        public IEnumerable<string> GetAllCategories(IList<PicasaIniData> picasaIniDataFiles)
        {
            return picasaIniDataFiles
                .SelectMany(i => i.Categories)
                .Union(picasaIniDataFiles.SelectMany(j => j.P2Categories))
                .Distinct();
        }

        public Dictionary<string, PicasaAlbum> GetAllAlbums(IList<PicasaIniData> picasaIniDataFiles)
        {
            var allAlbums =
                from iniData in picasaIniDataFiles
                from album in iniData.Albums
                select album;

            var dic = new Dictionary<string, PicasaAlbum>();
            foreach (var albumPair in allAlbums)
            {
                if(!dic.ContainsKey(albumPair.Key))
                    dic.Add(albumPair.Key, albumPair.Value);
            }
            
            return dic;

        }

    }
}
