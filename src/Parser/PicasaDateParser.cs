using System;
using System.Globalization;

namespace DotnetFer.PicasaParser
{
    public static class PicasaDateParser
    {
        private static readonly DateTime StartingDate = new DateTime(1900, 1, 1);
        private static readonly NumberFormatInfo PicasaNumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

        public static DateTime PicasaDateToDateTime(string dateString)
        {
            return StartingDate.AddDays(double.Parse(dateString, PicasaNumberFormat) - 2);
        }
    }
}
