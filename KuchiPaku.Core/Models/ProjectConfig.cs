using System.Collections.Generic;
using Tomlyn;

namespace KuchiPaku.Models;

public class ProjectConfig
{
    public Dictionary<string, CharaConfig> Characters { get; set; } = [];

    public static string Serialize(ProjectConfig config)
    {
        return Toml.FromModel(config);
    }

    public static ProjectConfig Deserialize(string toml)
    {
        return Toml.ToModel<ProjectConfig>(toml);
    }
}

public class CharaConfig
{
    public bool IsExport { get; set; } = true;
    public int ConsonantOption { get; set; } = 1;
    public Dictionary<string, string> PhonemeMap { get; set; } = [];
}
