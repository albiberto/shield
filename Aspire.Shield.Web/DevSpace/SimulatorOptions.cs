namespace Aspire.Shield.Web.DevSpace;

public class SimulatorOptions
{
    public IReadOnlyList<string> Branches { get; } = ["Trieste", "Gallarate", "Padova"];
    public IReadOnlyList<string> BusinessUnits { get; } = ["Finance", "IT", "Marketing", "Sales"];
}