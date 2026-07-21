using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;

namespace LordCaptor
{
    [HarmonyPatch(typeof(TaleWorlds.CampaignSystem.Encounters.PlayerEncounter), "EncounterState", MethodType.Setter)]
    public static class BlockCaptureStatePatch
    {
        public static void Prefix(ref TaleWorlds.CampaignSystem.Encounters.PlayerEncounterState value)
        {
            if (value == TaleWorlds.CampaignSystem.Encounters.PlayerEncounterState.CaptureHeroes && !LordCaptorManager.BattleHandled)
            {
                LordCaptorManager.Log("[STATE] EncounterState changing to CaptureHeroes. Intercepting...");
                
                try
                {
                    var rosterProp = typeof(TaleWorlds.CampaignSystem.Encounters.PlayerEncounter).GetProperty("RosterToReceiveLootPrisoners", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (rosterProp != null)
                    {
                        var roster = rosterProp.GetValue(TaleWorlds.CampaignSystem.Encounters.PlayerEncounter.Current) as TaleWorlds.CampaignSystem.Roster.TroopRoster;
                        if (roster != null)
                        {
                            List<Hero> heroes = new List<Hero>();
                            List<TaleWorlds.CampaignSystem.CharacterObject> heroesToRemove = new List<TaleWorlds.CampaignSystem.CharacterObject>();
                            foreach (var troop in roster.GetTroopRoster())
                            {
                                if (troop.Character != null && troop.Character.IsHero && troop.Character.HeroObject != null && troop.Character.HeroObject.IsAlive)
                                {
                                    heroes.Add(troop.Character.HeroObject);
                                    heroesToRemove.Add(troop.Character);
                                    LordCaptorManager.Log($"[STATE] Extracted hero from Roster: {troop.Character.Name}");
                                }
                            }
                            
                            foreach (var c in heroesToRemove)
                            {
                                roster.RemoveTroop(c, roster.GetTroopCount(c));
                            }
                            
                            if (heroes.Count > 0)
                            {
                                LordCaptorManager.Log("[STATE] Triggering LordCaptorManager.StartFlow");
                                LordCaptorManager.BattleHandled = true;
                                LordCaptorManager.SkipVanillaCaptureLogic = true;
                                LordCaptorManager.StartFlow(heroes);
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    LordCaptorManager.Log("[ERROR] Failed to extract heroes in State patch: " + ex.Message);
                }
                
                LordCaptorManager.Log("[STATE] Bypassing CaptureHeroes and jumping to FreeHeroes!");
                value = TaleWorlds.CampaignSystem.Encounters.PlayerEncounterState.FreeHeroes;
            }
        }
    }
}
