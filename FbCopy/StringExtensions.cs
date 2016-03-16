namespace FbCopy
{
    static class StringExtensions
    {
        public static string Quote(this string x)
        {
            return "\"" + x + "\"";
        }
    }
}
