using System;
using Il2Cpp;
using Il2CppGame;
using Il2CppGR;
using MelonLoader;
using UnityEngine;

namespace DeathNoteCheaterWithin.Features.Game
{
    public static class LevelManager
    {
        public static float ExpToAdd { get; set; } = 2500f;

        // Adds EXP to both in-match Role/Character Level AND OutGame BattlePass
        public static void AddPlayerExp(float amount, InGameExpType type = (InGameExpType)0)
        {
            try
            {
                bool processed = false;
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;

                // 1. In-Match Player & Team LEVEL Injection
                if (gsm != null && gsm.MyPlayer != null)
                {
                    PlayerMovement p = gsm.MyPlayer;
                    int prevExp = (int)p.CurrentExp;
                    p.TodayGainedExp += amount;
                    p.CurrentExp += amount;

                    // Increase level up to max
                    int maxLv = p.MaxLevel > 0 ? p.MaxLevel : 10;
                    int newLv = Math.Min(maxLv, p.TodayGainedLevel + 1);
                    p.TodayGainedLevel = newLv;

                    try
                    {
                        p.AddExp(amount, type);
                    }
                    catch { }

                    if (p.LevelData != null)
                    {
                        try
                        {
                            p.CheckLevelUpStatChange(p.LevelData, newLv);
                        }
                        catch { }
                    }

                    try
                    {
                        p.InvokeOnAddExpEvent(prevExp, newLv, type);
                    }
                    catch { }

                    // Sync to match team gauge
                    try
                    {
                        var glgm = GroupLevelGaugeManager.Instance;
                        if (glgm != null)
                        {
                            glgm.AddPlayerLevel(type, 1);
                            glgm.AddPlayerExp(type, (int)amount);
                        }
                    }
                    catch { }

                    MelonLogger.Msg($"[LevelManager] Injected +{amount:F0} EXP into match role {p.Role} ({p.RoleDetail})! Match Level: {p.CurrentLevel}/{maxLv}, Exp: {p.CurrentExp:F0}");
                    processed = true;
                }

                // 2. Out-Game / Lobby BattlePass & SaveData
                try
                {
                    var sdm = UnityEngine.Object.FindObjectOfType<SaveDataManager>();
                    if (sdm != null && sdm.CurrentSaveData != null)
                    {
                        var curPass = sdm.CurrentSaveData.GetCurrentBattlePass();
                        if (curPass != null)
                        {
                            curPass.PassExp += (int)amount;
                            sdm.StartSaveGame();
                            MelonLogger.Msg($"[LevelManager] BattlePass '{curPass.Pass_ID}' EXP injected: +{amount:F0} (New Total: {curPass.PassExp}) - Saved!");
                            processed = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[LevelManager] BattlePass injection note: {ex.Message}");
                }

                // 3. UI Refreshes
                RefreshAllLevelUIs();

                if (!processed)
                {
                    MelonLogger.Msg($"[LevelManager] EXP injection dispatched (+{amount:F0}) - waiting for match or lobby load.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LevelManager] AddPlayerExp error: {ex.Message}");
            }
        }

        // Instantly Maximize Both In-Match Role Level (Lv 10) AND BattlePass
        public static void SetInstantMaxLevel()
        {
            try
            {
                Config.ForceMaxLevel = true;
                Config.UnlockAllAbilities = true;

                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm != null && gsm.MyPlayer != null)
                {
                    PlayerMovement p = gsm.MyPlayer;
                    int prevExp = (int)p.CurrentExp;
                    int maxLv = p.MaxLevel > 0 ? p.MaxLevel : 10;
                    p.TodayGainedLevel = maxLv;
                    p.CurrentExp = 99999f;
                    p.TodayGainedExp = 99999f;

                    try
                    {
                        p.AddExp(10000f, (InGameExpType)0);
                    }
                    catch { }

                    if (p.LevelData != null)
                    {
                        try
                        {
                            p.CheckLevelUpStatChange(p.LevelData, maxLv);
                        }
                        catch { }
                    }

                    try
                    {
                        p.InvokeOnAddExpEvent(prevExp, maxLv, (InGameExpType)0);
                    }
                    catch { }

                    // Sync to match team gauge
                    try
                    {
                        var glgm = GroupLevelGaugeManager.Instance;
                        if (glgm != null)
                        {
                            glgm.AddGroupBaseLevel((InGameExpType)0, maxLv);
                            glgm.AddPlayerLevel((InGameExpType)0, maxLv);
                            glgm.AddPlayerExp((InGameExpType)0, 99999);
                        }
                    }
                    catch { }

                    MelonLogger.Msg($"[LevelManager] Match Role Level instantly MAXED OUT to Level {maxLv}! All abilities & cards active.");
                }

                // Max out BattlePass
                try
                {
                    var sdm = UnityEngine.Object.FindObjectOfType<SaveDataManager>();
                    if (sdm != null && sdm.CurrentSaveData != null)
                    {
                        var curPass = sdm.CurrentSaveData.GetCurrentBattlePass();
                        if (curPass != null)
                        {
                            curPass.PassExp = Math.Max(curPass.PassExp, 500000);
                            for (int lv = 1; lv <= 100; lv++)
                            {
                                try
                                {
                                    curPass.GainReward(lv, true);
                                    curPass.GainReward(lv, false);
                                }
                                catch { }
                            }
                            sdm.StartSaveGame();
                            MelonLogger.Msg($"[LevelManager] BattlePass '{curPass.Pass_ID}' maxed out to 500k EXP and all rewards unlocked!");
                        }
                    }
                }
                catch { }

                RefreshAllLevelUIs();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LevelManager] SetInstantMaxLevel error: {ex.Message}");
            }
        }

        // Grant EXP to all active players in match (Host Authority)
        public static void AddExpToAll(float amount)
        {
            try
            {
                GameStateManager gsm = EngineStrictSingleton<GameStateManager>.Instance;
                if (gsm == null || gsm.PlayerList == null)
                {
                    MelonLogger.Warning("[LevelManager] Mass EXP injection requires an active match player roster.");
                    return;
                }

                var enumerator = gsm.PlayerList.GetEnumerator();
                int count = 0;
                while (enumerator.MoveNext())
                {
                    PlayerMovement p = enumerator.Current;
                    if (p != null)
                    {
                        int prev = (int)p.CurrentExp;
                        p.TodayGainedExp += amount;
                        p.CurrentExp += amount;
                        p.TodayGainedLevel = Math.Min(p.MaxLevel > 0 ? p.MaxLevel : 10, p.TodayGainedLevel + 1);
                        try
                        {
                            p.AddExp(amount, (InGameExpType)0);
                        }
                        catch { }

                        if (p.LevelData != null)
                        {
                            try
                            {
                                p.CheckLevelUpStatChange(p.LevelData, p.TodayGainedLevel);
                            }
                            catch { }
                        }
                        try
                        {
                            p.InvokeOnAddExpEvent(prev, p.TodayGainedLevel, (InGameExpType)0);
                        }
                        catch { }
                        count++;
                    }
                }
                MelonLogger.Msg($"[LevelManager] Injected +{amount:F0} EXP to all {count} match players!");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LevelManager] AddExpToAll error: {ex.Message}");
            }
        }

        public static void RefreshAllLevelUIs()
        {
            try
            {
                if (EngineSingleton<Il2CppLunaOutGame.CharacterEdit>.Instance != null)
                {
                    EngineSingleton<Il2CppLunaOutGame.CharacterEdit>.Instance.RefreshUI();
                }
            }
            catch { }

            try
            {
                var footer = UnityEngine.Object.FindObjectOfType<Il2CppLunaOutGame.FooterBattlePassUI>();
                if (footer != null)
                {
                    footer.RefreshUI();
                }
            }
            catch { }
        }
    }
}
