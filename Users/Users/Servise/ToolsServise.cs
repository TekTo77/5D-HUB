namespace Servise.Tools
{
    public static class Tools
    {

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}
