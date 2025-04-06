namespace Servise.Tools
{
    public static class ToolsServise
    {

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}
