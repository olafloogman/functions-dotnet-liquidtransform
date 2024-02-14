namespace DotLiquid.Extensible.AzFunc.v4
{
    public static class ContentFactory
    {
        public static IContentReader GetContentReader(string contentType)
        {
            switch (contentType)
            {
                case "application/xml":
                case "text/xml":
                    return new XmlContentReader();
                case "text/csv":
                    return new CsvContentReader();
                default:
                    return new JsonContentReader();
            }
        }

        public static IContentWriter GetContentWriter(string contentType)
        {
            switch (contentType)
            {
                case "application/json":
                    return new JsonContentWriter(contentType);
                default:
                    return new BasicContentWriter(contentType);
            }
        }
    }
}
