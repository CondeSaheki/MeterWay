namespace MeterWay.Managers;

public class ConfigurationManager
{
    public readonly Configuration Configuration;

    public static ConfigurationManager Inst { get; private set; } = null!;

    public ConfigurationManager()
    {
        Configuration = InterfaceManager.Inst.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(InterfaceManager.Inst.PluginInterface);

        Inst = this;
    }
}