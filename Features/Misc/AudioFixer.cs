using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using Il2CppGRNetwork;
using MelonLoader;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Misc
{
    public static class AudioFixer
    {
        // =========================================================================
        // 1. UNITY AUDIO ENGINE STABILIZATION
        // =========================================================================

        [HarmonyPatch(typeof(AudioListener), nameof(AudioListener.pause), MethodType.Getter)]
        public static class AudioListener_pause_Getter_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (Config.EnableAudioFix && __result)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(AudioListener), nameof(AudioListener.pause), MethodType.Setter)]
        public static class AudioListener_pause_Setter_Patch
        {
            public static bool Prefix(ref bool value)
            {
                if (Config.EnableAudioFix && value)
                {
                    value = false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(AudioListener), nameof(AudioListener.volume), MethodType.Setter)]
        public static class AudioListener_volume_Setter_Patch
        {
            public static void Prefix(ref float value)
            {
                if (Config.EnableAudioFix && value < 0.2f)
                {
                    value = 1.0f;
                }
            }
        }

        [HarmonyPatch(typeof(AudioSource), nameof(AudioSource.Play), new Type[0])]
        public static class AudioSource_Play_Patch
        {
            public static void Prefix(AudioSource __instance)
            {
                if (Config.EnableAudioFix && __instance != null)
                {
                    if (__instance.mute) __instance.mute = false;
                    if (__instance.volume < 0.1f) __instance.volume = 1.0f;
                }
            }
        }

        // =========================================================================
        // 2. VOICE CHAT (VIVOX / PHOTON) CUTOUT & OCCLUSION FIXES
        // =========================================================================

        // Bypasses line-of-sight raycast so voices NEVER cut out behind walls, doors, or corners
        [HarmonyPatch(typeof(PlayerVCController), nameof(PlayerVCController.CanSeePlayer))]
        public static class VoiceChat_CanSeePlayer_Patch
        {
            public static bool Prefix(ref bool __result, ref float distance)
            {
                if (Config.EnableAudioFix)
                {
                    distance = 1.0f; // Force line-of-sight distance to close range
                    __result = true; // Always visible -> no wall occlusion cutout
                    return false;
                }
                return true;
            }
        }

        // Maximizes and stabilizes player voice volume across all distances and rooms
        [HarmonyPatch(typeof(PlayerVCController), nameof(PlayerVCController.SetPlayerAudioLevel), new Type[] { typeof(PlayerMovement), typeof(int) })]
        public static class PlayerVCController_SetPlayerAudioLevel_Patch
        {
            public static void Prefix(PlayerVCController __instance, PlayerMovement player, ref int baseVolume)
            {
                if (Config.EnableAudioFix && baseVolume < 10)
                {
                    baseVolume = 10; // Maximum volume and clarity
                }
            }
        }

        // Prevents system transitions (meetings, room changes, cutscenes) from muting Voice Chat
        [HarmonyPatch(typeof(PlayerVCController), nameof(PlayerVCController.SetMuteBySystem))]
        public static class VoiceChat_PlayerVC_SetMuteBySystem_Patch
        {
            public static bool Prefix(ref bool set)
            {
                if (Config.EnableAudioFix && set)
                {
                    set = false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerVCController), nameof(PlayerVCController.SetMuteByPlayer))]
        public static class VoiceChat_PlayerVC_SetMuteByPlayer_Patch
        {
            public static bool Prefix(ref bool set)
            {
                if (Config.EnableAudioFix && set)
                {
                    set = false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(VoiceChatManager), nameof(VoiceChatManager.SetMuteBySystem))]
        public static class VoiceChat_Manager_SetMuteBySystem_Patch
        {
            public static bool Prefix(ref bool set)
            {
                if (Config.EnableAudioFix && set)
                {
                    set = false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(VoiceChatManager), nameof(VoiceChatManager.SetMuteByPlayer))]
        public static class VoiceChat_Manager_SetMuteByPlayer_Patch
        {
            public static bool Prefix(ref bool set)
            {
                if (Config.EnableAudioFix && set)
                {
                    set = false;
                }
                return true;
            }
        }

        // Prevents voice chat error state from shutting down audio channels on packet loss / jitter
        [HarmonyPatch(typeof(PlayerVCController), nameof(PlayerVCController.SetVCErrorMode))]
        public static class VoiceChat_SetVCErrorMode_Patch
        {
            public static bool Prefix()
            {
                if (Config.EnableAudioFix)
                {
                    return false; // Suppress VC error state shutdown
                }
                return true;
            }
        }

        // Lowers microphone noise gate threshold so quiet speech is never clipped or dropped mid-sentence
        [HarmonyPatch(typeof(VoiceChatManager), nameof(VoiceChatManager.SetDetectionThreshold))]
        public static class VoiceChat_SetDetectionThreshold_Patch
        {
            public static void Prefix(ref float thresh)
            {
                if (Config.EnableAudioFix)
                {
                    thresh = 0.005f; // Highly sensitive threshold
                }
            }
        }

        // =========================================================================
        // 3. CONTINUOUS STABILIZATION (STATE-BASED, ZERO PACKET DROP THRASHING)
        // =========================================================================

        private static int _lastKnownGameState = -999;
        private static bool _lastGhostsEnabled = false;

        public static void Update()
        {
            if (!Config.EnableAudioFix) return;

            try
            {
                if (!Application.runInBackground)
                {
                    Application.runInBackground = true;
                }

                if (AudioListener.pause)
                {
                    AudioListener.pause = false;
                }

                if (AudioListener.volume < 0.2f)
                {
                    AudioListener.volume = 1.0f;
                }

                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                int currentState = gsm != null ? (int)gsm.State : -1;
                bool ghostsChanged = Config.HearGhostsForAll != _lastGhostsEnabled;

                // ONLY configure listen groups on actual match state transition or config toggle!
                // Never thrash AddTargetListenGroup in a timer loop, which drops incoming Vivox frames!
                if (currentState != _lastKnownGameState || ghostsChanged)
                {
                    _lastKnownGameState = currentState;
                    _lastGhostsEnabled = Config.HearGhostsForAll;

                    try
                    {
                        if (Config.HearGhostsForAll)
                        {
                            var groups = VoiceChatManager.GetTargetListenGroups();
                            bool hasDead = false;
                            if (groups != null)
                            {
                                for (int i = 0; i < groups.Length; i++)
                                {
                                    if (groups[i] == (int)EVCGroups.Dead) { hasDead = true; break; }
                                }
                            }
                            if (!hasDead)
                            {
                                VoiceChatManager.AddTargetListenGroup((int)EVCGroups.Dead);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void ResetAudioEngine()
        {
            try
            {
                AudioListener.pause = false;
                AudioListener.volume = 1.0f;
                AudioConfiguration config = AudioSettings.GetConfiguration();
                config.dspBufferSize = 512;
                AudioSettings.Reset(config);
                UnmuteAllSceneAudio();
                VoiceChatManager.VoiceRefreshBoost();
                MelonLogger.Msg("[+] Audio Engine, CriWare, & Voice Chat pipeline reinitialized successfully!");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] Audio engine reset failed: {ex.Message}");
            }
        }

        public static int UnmuteAllSceneAudio()
        {
            int count = 0;
            try
            {
                AudioSource[] sources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
                if (sources != null)
                {
                    foreach (var src in sources)
                    {
                        if (src != null)
                        {
                            if (src.mute) { src.mute = false; count++; }
                            if (src.volume < 0.2f) { src.volume = 1.0f; }
                        }
                    }
                }
                MelonLogger.Msg($"[+] Unmuted {count} AudioSource components in active scene.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] UnmuteAllSceneAudio failed: {ex.Message}");
            }
            return count;
        }
    }
}
