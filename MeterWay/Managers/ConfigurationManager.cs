//namespace MeterWay.Configuration;
namespace MeterWay.Managers;

public class ConfigurationManager
{
    public readonly Configuration Configuration;

    public static ConfigurationManager Inst { get; private set; } = null!;

    public ConfigurationManager()
    {
        Configuration = Configuration.Load();
        Inst = this;
    }
}