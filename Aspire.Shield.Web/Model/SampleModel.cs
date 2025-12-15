namespace Aspire.Shield.Web.Model;

public abstract record SampleModel(string BusinessUnit, string Branch)
{
    public string Key => $"{BusinessUnit}-{Branch}";
    
    public sealed record WithCount(string BusinessUnit, string Branch, int Count) : SampleModel(BusinessUnit, Branch);
    
    public sealed record WithState(string BusinessUnit, string Branch, StateEnum State) : SampleModel(BusinessUnit, Branch);
    
    public enum StateEnum
    {
        Enqueued,
        Processing,
        Completed
    }
}