namespace AspNetSample
{
    public class AppTenant
    {
        public string Name { get; set; }
        public IEnumerable<string> Hostnames { get; set; }
    }
}