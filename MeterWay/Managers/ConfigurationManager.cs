namespace MeterWay.Managers;

public class ConfigurationManager
{
    public readonly Configuration Configuration;

    public static ConfigurationManager Inst { get; private set; } = null!;

    public ConfigurationManager()
    {
        Configuration = Dalamud.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(Dalamud.PluginInterface);

        Inst = this;
    }
}