using System;
using System.Threading.Tasks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SaboTender;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextCommand TextCommand { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;

    private const string CommandName = "/autogardener";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("AutoGardenerPlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    // Gardening menu addon names - these may need adjustment based on actual game data
    private readonly string[] GardeningAddonNames = { "SelectString", "ContextMenu" };

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        TextCommand.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Auto Gardener configuration window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Hook into addon lifecycle to detect gardening menus
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, GardeningAddonNames, OnGardeningMenuSetup);
        
        PluginLog.Information("Auto Gardener Plugin loaded!");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        TextCommand.RemoveHandler(CommandName);
        AddonLifecycle.UnregisterListener(OnGardeningMenuSetup);
    }

    private void OnCommand(string command, string args)
    {
        ToggleConfigUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    private unsafe void OnGardeningMenuSetup(AddonEvent type, AddonArgs args)
    {
        if (!Configuration.EnableAutoSelect)
            return;

        try
        {
            var addon = (AtkUnitBase*)args.Addon;
            if (addon == null)
                return;

            // Check if this is actually a gardening context menu
            if (!IsGardeningMenu(addon))
                return;

            PluginLog.Debug($"Detected gardening menu: {args.AddonName}");

            // Schedule the auto-selection after a short delay to ensure the menu is fully loaded
            Task.Delay(Configuration.DelayMs).ContinueWith(_ =>
            {
                Framework.RunOnTick(() => SelectTendToCropOption(addon));
            });
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Error in gardening menu detection");
        }
    }

    private unsafe bool IsGardeningMenu(AtkUnitBase* addon)
    {
        if (addon == null || addon->RootNode == null)
            return false;

        // Check if the menu contains gardening-related text
        // This is a simplified check - you may need to adjust based on actual game data
        var textNodeCount = addon->UldManager.NodeListCount;
        
        for (var i = 0; i < textNodeCount; i++)
        {
            var node = addon->UldManager.NodeList[i];
            if (node == null || node->Type != NodeType.Text)
                continue;

            var textNode = (AtkTextNode*)node;
            if (textNode->NodeText.ToString().Contains("Tend") ||
                textNode->NodeText.ToString().Contains("Garden") ||
                textNode->NodeText.ToString().Contains("Crop"))
            {
                return true;
            }
        }

        return false;
    }

    private unsafe void SelectTendToCropOption(AtkUnitBase* addon)
    {
        try
        {
            if (addon == null || !addon->IsVisible)
                return;

            // For SelectString addon, we want to click the second option (index 1)
            // This simulates clicking on the "Tend to crop" option
            var values = stackalloc AtkValue[2];
            values[0] = new AtkValue { Type = ValueType.Int, Int = 1 }; // Option index (0-based)
            values[1] = new AtkValue { Type = ValueType.UInt, UInt = 0 }; // Additional parameter

            addon->FireCallback(2, values);
            
            PluginLog.Information("Auto-selected 'Tend to crop' option");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Error auto-selecting gardening option");
        }
    }
}

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool EnableAutoSelect { get; set; } = true;
    public int DelayMs { get; set; } = 100; // Delay before auto-selecting in milliseconds

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}