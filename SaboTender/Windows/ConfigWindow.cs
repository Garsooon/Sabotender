using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SaboTender;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for the window to be reopened and maintain its state
    public ConfigWindow(Plugin plugin) : base("Auto Gardener Configuration###AutoGardenerConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(350, 200);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Enable/Disable toggle
        var enableAutoSelect = Configuration.EnableAutoSelect;
        if (ImGui.Checkbox("Enable Auto-Select 'Tend to Crop'", ref enableAutoSelect))
        {
            Configuration.EnableAutoSelect = enableAutoSelect;
            Configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("When enabled, automatically selects 'Tend to crop' when gardening menus appear");
        }

        ImGui.Spacing();

        // Delay setting
        var delayMs = Configuration.DelayMs;
        if (ImGui.SliderInt("Selection Delay (ms)", ref delayMs, 50, 1000))
        {
            Configuration.DelayMs = delayMs;
            Configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Delay before auto-selecting the option. Increase if experiencing issues.");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Status information
        ImGui.Text("Status:");
        ImGui.SameLine();
        if (Configuration.EnableAutoSelect)
        {
            ImGui.TextColored(new Vector4(0, 1, 0, 1), "Active");
        }
        else
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Disabled");
        }

        ImGui.Spacing();
        ImGui.TextWrapped("This plugin will automatically select the 'Tend to crop' option when you interact with plants in your garden.");
        
        ImGui.Spacing();
        ImGui.TextColored(new Vector4(1, 1, 0, 1), "Note:");
        ImGui.TextWrapped("If the plugin doesn't work as expected, try adjusting the delay or check the Dalamud log for errors.");
    }
}