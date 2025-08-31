using System.Globalization;


namespace pdf2eink
{
    public static class Extensions
    {
        public static double ToDouble(this string p)
        {
            return double.Parse(p.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        public static float ToFloat(this string p)
        {
            return float.Parse(p.Replace(",", "."), CultureInfo.InvariantCulture);
        }
    }

    
}
