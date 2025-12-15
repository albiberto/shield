namespace Aspire.Shield.Web.DevSpace;

public class SimulatorOptions
{
    public IReadOnlyList<string> Branches { get; } = ["Trieste", "Gallarate", "Padova"];
    public IReadOnlyList<string> BusinessUnits { get; } = ["Finance", "IT", "Marketing", "Sales"];
    
    public string GetRandomBranch() => Branches[Random.Shared.Next(Branches.Count)];
    public string GetRandomBusinessUnit() => BusinessUnits[Random.Shared.Next(BusinessUnits.Count)];
}