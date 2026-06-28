using SPTarkov.Server.Core.Models.Spt.Mod;

namespace KeysMapInfo4;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.keysmapinfo4.server";
    public override string Name { get; init; } = "KeysMapInfo4";
    public override string Author { get; init; } = "KeysMapInfo Team";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.3.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}
