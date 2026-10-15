using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppGame;
using MelonLoader;

namespace DeathNoteCheaterWithin.Features.Game
{
    public static class MeetingHacks
    {
        [HarmonyPatch(typeof(MeetingPlayer), nameof(MeetingPlayer.Update))]
        public static class MeetingPlayer_Update
        {
            public static void Prefix(MeetingPlayer __instance)
            {
                if (Config.VoteManyTimes && __instance != null)
                {
                    try
                    {
                        __instance.SetInteractMeetingButtons((MeetingPlayerButton.EInteractableType)2, false);
                        __instance.SetActiveMeetingButtons((MeetingPlayerButton.EActiveType)4, false);
                    }
                    catch { }
                }
            }
        }

        public static void CallEmergencyMeeting()
        {
            try
            {
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm != null)
                {
                    gsm.SendCallEmergencyMeeting();
                    MelonLogger.Msg("[+] Dispatched emergency meeting call to game state!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] CallEmergencyMeeting failed: {ex.Message}");
            }
        }

        public static void SkipMeetingTimer()
        {
            try
            {
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm == null) return;

                // Fast-forward meeting phase state
                if ((int)gsm.State == 3 || (int)gsm.State == 4)
                {
                    gsm.SendCallEmergencyMeeting();
                    MelonLogger.Msg("[+] Fast-forwarded meeting discussion timer!");
                }
                else
                {
                    MelonLogger.Warning("[MeetingHacks] SkipMeetingTimer: Active meeting phase not detected.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[-] SkipMeetingTimer failed: {ex.Message}");
            }
        }
    }
}
