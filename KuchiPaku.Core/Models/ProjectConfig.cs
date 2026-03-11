using System.Collections.Generic;

namespace KuchiPaku.ModelsData
{
    public class ProjectConfig
    {
        public Dictionary<string, CharaConfig> Characters { get; set; } = new Dictionary<string, CharaConfig>();

        public static string Serialize(ProjectConfig config)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
        }

        public static ProjectConfig Deserialize(string json)
        {
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectConfig>(json);
            return result ?? new ProjectConfig();
        }
    }

    public class CharaConfig
    {
        public bool IsExport { get; set; } = true;
        public int ConsonantOption { get; set; } = 1;
        public Dictionary<string, string> PhonemeMap { get; set; } = new Dictionary<string, string>();
    }
}
