using System;
using System.Collections.Generic;
using DeathNoteCheaterWithin.Drawing;
using DeathNoteCheaterWithin.Features.ESP;
using Il2Cpp;
using Il2CppGame;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.ESP
{
    public static class EspCache
    {
        private static float _lastScanTime = 0f;
        private const float ScanInterval = 0.25f; // Scan 4 times per second to optimize FPS

        public static List<NPCController> NpcCache { get; } = new List<NPCController>();
        public static List<InteractableObject> InteractableCache { get; } = new List<InteractableObject>();

        public static void UpdateCache()
        {
            if (Time.time - _lastScanTime < ScanInterval) return;
            _lastScanTime = Time.time;

            // Cache NPCs
            NpcCache.Clear();
            if (Config.NpcEsp || Config.NpcCorpseEsp)
            {
                var npcs = UnityEngine.Object.FindObjectsOfType<NPCController>();
                if (npcs != null)
                {
                    foreach (var npc in npcs)
                    {
                        if (npc != null) NpcCache.Add(npc);
                    }
                }
            }

            // Cache Interactables
            InteractableCache.Clear();
            if (Config.TaskEsp)
            {
                var items = UnityEngine.Object.FindObjectsOfType<InteractableObject>();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item != null) InteractableCache.Add(item);
                    }
                }
            }
        }
    }
}
