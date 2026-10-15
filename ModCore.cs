using System;
using DeathNoteCheaterWithin.Features.Combat;
using DeathNoteCheaterWithin.Features.Dev;
using DeathNoteCheaterWithin.Features.ESP;
using DeathNoteCheaterWithin.Features.Game;
using DeathNoteCheaterWithin.Features.Movement;
using DeathNoteCheaterWithin.Features.Roles;
using DeathNoteCheaterWithin.UI;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using Il2CppGame;

[assembly: MelonInfo(typeof(DeathNoteCheaterWithin.ModCore), "DeathNoteCheaterWithin", "3.0.0", "Inkwell")]
[assembly: MelonGame("BANDAI NAMCO Entertainment", "DEATH NOTE Killer Within")]

namespace DeathNoteCheaterWithin
{
    public class ModCore : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Config.LoadConfig();
            MelonLogger.Msg("=================================================================");
            MelonLogger.Msg("  DEATH NOTE Cheater Within v3.0 - Ultimate Tactical Mod Loaded");
            MelonLogger.Msg("  Developed by: Inkwell");
            MelonLogger.Msg("  Press [INSERT] to toggle Menu | Press [L] for Keybinds HUD");
            MelonLogger.Msg("  Press [P] to Dump Game State to Desktop");
            MelonLogger.Msg("=================================================================");
        }

        public override void OnUpdate()
        {
            // Menu Toggle Hotkey (INSERT Only)
            if (NativeInput.WasKeyPressed(NativeInput.VK_INSERT))
            {
                Config.MenuOpen = !Config.MenuOpen;
            }

            // Continuous Tick Processors
            NoClip.Update();
            CameraHacks.Update();
            InstantIdSteal.Update();
            Exploits.UpdateSelfHacks();
            Features.Misc.AudioFixer.Update();
            Features.Misc.CustomNameColor.Update();
        }

        public override void OnGUI()
        {
            PlayerEsp.Render();
            NpcEsp.Render();
            WorldEsp.Render();
            StateDumper.DrawDebugHud();
            TacticalMenu.Draw();
        }
    }
}
