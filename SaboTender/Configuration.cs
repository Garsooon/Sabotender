using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace SaboTender;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool EnableAutoSelect { get; set; } = true;
    public int DelayMs { get; set; } = 100; // Delay before auto-selecting in milliseconds

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }
}
