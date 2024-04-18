namespace Nananet.Sdk.Fanbook.Models.Results;

public class GetMyGuildsResult
{
    public List<Guild> Lists { get; set; } = null!;
    public string Hash { get; set; } = null!;
}