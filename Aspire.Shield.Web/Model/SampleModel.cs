namespace Aspire.Shield.Web.Model;

public record SampleModel(string BusinessUnit, string Branch, int Count)
{
    public string Key => $"{BusinessUnit}-{Branch}";
}