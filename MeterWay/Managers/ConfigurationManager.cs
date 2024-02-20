namespace MeterWay.Managers;

public class ConfigurationManager
{
    public readonly Configuration Configuration;

    public static ConfigurationManager Instance { get; private set; } = null!;

    public ConfigurationManager()
    {
        this.Configuration = InterfaceManager.Inst.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Configuration.Initialize(InterfaceManager.Inst.PluginInterface);

        ConfigurationManager.Instance = this;
    }
}