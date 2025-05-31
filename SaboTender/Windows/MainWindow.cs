using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace SaboTender;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "Auto Gardener" as window title,
    // but for ImGui the ID is "Auto Gardener##AutoGardenerMainWindow"
    public MainWindow(Plugin plugin)
        : base("Auto Gardener##AutoGardenerMainWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text($"Auto Gardener Plugin");

        ImGui.Spacing();

        ImGui.Text("Welcome to the Auto Gardener plugin!");
        ImGui.TextWrapped("This plugin automatically selects the 'Tend to crop' option when you interact with plants in your garden, saving you time and clicks.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Quick status display
        ImGui.Text("Current Status:");
        ImGui.SameLine();
        if (Plugin.Configuration.EnableAutoSelect)
        {
            ImGui.TextColored(new Vector4(0, 1, 0, 1), "✓ Active");
        }
        else
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "✗ Disabled");
        }

        ImGui.Spacing();

        // Quick toggle
        var enabled = Plugin.Configuration.EnableAutoSelect;
        if (ImGui.Checkbox("Enable Auto-Select", ref enabled))
        {
            Plugin.Configuration.EnableAutoSelect = enabled;
            Plugin.Configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Instructions
        ImGui.Text("How to use:");
        ImGui.BulletText("Make sure the plugin is enabled (green checkmark above)");
        ImGui.BulletText("Go to your garden and interact with any plant");
        ImGui.BulletText("The plugin will automatically select 'Tend to crop'");
        ImGui.BulletText("You can adjust settings in the configuration window");

        ImGui.Spacing();

        if (ImGui.Button("Open Configuration"))
        {
            Plugin.ToggleConfigUI();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Information section
        ImGui.TextColored(new Vector4(0.7f, 0.7f, 1f, 1f), "ℹ Information");
        ImGui.TextWrapped("This plugin hooks into FFXIV's addon system to detect when gardening menus appear. " +
                         "It then automatically clicks the second option, which should be 'Tend to crop' in most cases.");

        ImGui.Spacing();
        ImGui.TextColored(new Vector4(1, 1, 0, 1), "⚠ Note");
        ImGui.TextWrapped("If the plugin stops working after a game update, it may need to be updated to match " +
                         "changes in the game's UI system. Check for plugin updates or report issues to the developer.");
    }
}