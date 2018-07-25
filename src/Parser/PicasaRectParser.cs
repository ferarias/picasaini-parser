using System;
using System.Globalization;
using DotnetFer.PicasaParser.Domain;

namespace DotnetFer.PicasaParser
{
    public static class PicasaRectParser
    {
        /// <summary>
        /// Picasa uses a special string format to store crop boxes of
        /// detected faces and from an applied crop filters. The number encased 
        /// in the rect64() statement is a 64 bit hexadecimal number:
        /// </summary>
        /// <param name="rectData"></param>
        /// <returns></returns>
        public static Rectangle Parse(string r)
        {
            if (r.Length != 24)
                return null;

            var rectData = r.Substring(7, 16);

            var a = rectData.Substring(0, 4);
            var b = rectData.Substring(4, 4);
            var c = rectData.Substring(8, 4);
            var d = rectData.Substring(12, 4);

            var aa = ConverToByte(a);
            var bb = ConverToByte(b);
            var cc = ConverToByte(c);
            var dd = ConverToByte(d);

            return new Rectangle
            {
                Left = aa,
                Top = bb,
                Right = cc,
                Bottom = dd
            };
        }

        private static float ConverToByte(string s)
        {
            return (float)ushort.Parse(s, NumberStyles.HexNumber) / ushort.MaxValue;
        }


    }
}
