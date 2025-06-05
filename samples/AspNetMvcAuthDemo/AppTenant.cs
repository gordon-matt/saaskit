namespace AspNetMvcAuthSample;

public class AppTenant : IEquatable<AppTenant>
{
    public string Name { get; set; }
    public string[] Hostnames { get; set; }

    public string Id => Name.Replace(" ", "").ToLower();

    public bool Equals(AppTenant other) => other != null && other.Name.Equals(Name);

    public override bool Equals(object obj) => Equals(obj as AppTenant);

    public override int GetHashCode() => Name.GetHashCode();
}