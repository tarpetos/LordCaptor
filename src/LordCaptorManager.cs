using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Library;
using TaleWorlds.Core.ImageIdentifiers;

namespace LordCaptor
{
    public static class LordCaptorManager
    {
        public static bool BlockEncounterUpdate { get; private set; } = false;
        public static bool SkipVanillaCaptureLogic { get; set; } = false;
        public static bool BattleHandled { get; set; } = false;
        public static bool IsGatheringHeroes { get; set; } = false;
        public static bool IsMenuOpen { get; set; } = false;
        public static List<Hero> GatheredHeroes { get; set; } = new List<Hero>();
        private static List<Hero> _pendingLords = new List<Hero>();

        public static void Log(string message)
        {
            try
            {
                // TaleWorlds.Core.MBInformationManager.AddQuickInformation(new TaleWorlds.Localization.TextObject("LordCaptor: " + message), 0, null, null, "");
                
                // string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "Low", "LordCaptor.log");
                // System.IO.File.AppendAllText(path, System.DateTime.Now.ToString("HH:mm:ss.fff") + " - " + message + "\n");
            }
            catch { }
        }

        public static void StartFlow(List<Hero> lords)
        {
            Log("StartFlow called with " + lords.Count + " lords.");
            _pendingLords = lords;
            BlockEncounterUpdate = true;
            SkipVanillaCaptureLogic = true;
            ShowPrisonerManagementUI(lords, false);
        }

        public static void ShowPrisonerManagementUI(List<Hero> heroes, bool fromShiftO)
        {
            Log($"[MOD] ShowPrisonerManagementUI called with {heroes.Count} heroes. fromShiftO: {fromShiftO}");
            var inquiryElements = new List<InquiryElement>();

            inquiryElements.Add(new InquiryElement(
                "apply_to_all",
                new TaleWorlds.Localization.TextObject("{=lord_captor_apply_to_all}--- Apply To All ---").ToString(),
                null,
                true,
                ""
            ));

            foreach (var hero in heroes)
            {
                inquiryElements.Add(new InquiryElement(
                    hero,
                    hero.Name.ToString(),
                    new CharacterImageIdentifier(CharacterCode.CreateFrom(hero.CharacterObject)),
                    true,
                    ""
                ));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TaleWorlds.Localization.TextObject("{=lord_captor_prisoner_mgmt_title}Prisoner Management").ToString(),
                new TaleWorlds.Localization.TextObject("{=lord_captor_prisoner_mgmt_desc}Select prisoners to manage:").ToString(),
                inquiryElements,
                true,
                fromShiftO ? 1 : 0,
                inquiryElements.Count,
                new TaleWorlds.Localization.TextObject("{=lord_captor_next}Next").ToString(),
                new TaleWorlds.Localization.TextObject("{=lord_captor_cancel}Cancel").ToString(),
                (List<InquiryElement> selected) =>
                {
                    bool applyToAll = false;
                    foreach (var s in selected)
                    {
                        if (s.Identifier is string str && str == "apply_to_all")
                        {
                            applyToAll = true;
                            break;
                        }
                    }

                    if (applyToAll)
                    {
                        ShowApplyToAllUI(heroes, fromShiftO);
                        return;
                    }

                    selected.RemoveAll(s => s.Identifier is string str && str == "apply_to_all");

                    if (selected.Count == 0)
                    {
                        if (!fromShiftO) 
                        {
                            foreach (var h in heroes)
                            {
                                if (h.IsAlive && PartyBase.MainParty != null) 
                                {
                                    TakePrisonerAction.Apply(PartyBase.MainParty, h);
                                }
                            }
                            SkipVanillaCaptureLogic = false;
                            BattleHandled = false;
                        }
                        IsMenuOpen = false;
                        return;
                    }
                    List<Hero> selectedHeroes = new List<Hero>();
                    foreach (var s in selected) selectedHeroes.Add(s.Identifier as Hero);
                    ShowPrisonerManagementActionUI(selectedHeroes, heroes, fromShiftO);
                },
                (List<InquiryElement> selected) =>
                {
                    if (!fromShiftO) 
                    {
                        foreach (var h in heroes)
                        {
                            if (h.IsAlive && PartyBase.MainParty != null) 
                            {
                                TakePrisonerAction.Apply(PartyBase.MainParty, h);
                            }
                        }
                        SkipVanillaCaptureLogic = false;
                        BattleHandled = false;
                    }
                    IsMenuOpen = false;
                }
            ));
        }

        private static void ShowPrisonerManagementActionUI(List<Hero> selectedHeroes, List<Hero> allHeroes, bool fromShiftO)
        {
            List<InquiryElement> options = new List<InquiryElement>();
            options.Add(new InquiryElement("execute", new TextObject("{=lord_captor_execute_all}Execute Selected").ToString(), null, true, ""));
            options.Add(new InquiryElement("release", new TextObject("{=lord_captor_release_all}Release Selected").ToString(), null, true, ""));
            options.Add(new InquiryElement("back", new TextObject("{=lord_captor_cancel}Back").ToString(), null, true, ""));

            TextObject descText = new TextObject("{=lord_captor_manage_desc}What would you like to do with the {COUNT} selected lords?");
            descText.SetTextVariable("COUNT", selectedHeroes.Count);

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=lord_captor_manage_title}Manage Prisoners").ToString(),
                descText.ToString(),
                options, true, 1, 1,
                new TextObject("{=lord_captor_continue}Continue").ToString(),
                "",
                (List<InquiryElement> selected) =>
                {
                    string choice = selected[0].Identifier as string;
                    if (choice == "back")
                    {
                        ShowPrisonerManagementUI(allHeroes, fromShiftO);
                        return;
                    }
                    else if (choice == "execute")
                    {
                        foreach (Hero h in selectedHeroes)
                        {
                            if (fromShiftO)
                            {
                                if (PartyBase.MainParty != null && PartyBase.MainParty.PrisonRoster != null)
                                    PartyBase.MainParty.PrisonRoster.RemoveTroop(h.CharacterObject, 1);
                            }
                            else
                            {
                                TakePrisonerAction.Apply(PartyBase.MainParty, h);
                            }
                            
                            KillCharacterAction.ApplyByExecution(h, Hero.MainHero, true, true);
                            allHeroes.Remove(h);
                        }
                    }
                    else if (choice == "release")
                    {
                        foreach (Hero h in selectedHeroes)
                        {
                            if (fromShiftO)
                            {
                                if (PartyBase.MainParty != null && PartyBase.MainParty.PrisonRoster != null)
                                    PartyBase.MainParty.PrisonRoster.RemoveTroop(h.CharacterObject, 1);
                            }
                            else
                            {
                                TakePrisonerAction.Apply(PartyBase.MainParty, h);
                            }
                            
                            if (!fromShiftO) 
                            {
                                EndCaptivityAction.ApplyByReleasedAfterBattle(h);
                            }
                            else
                            {
                                EndCaptivityAction.ApplyByReleasedByChoice(h, Hero.MainHero);
                            }
                            allHeroes.Remove(h);
                        }
                    }
                    

                    if (!fromShiftO)
                    {
                        foreach (var h in allHeroes)
                        {
                            if (h.IsAlive && PartyBase.MainParty != null) 
                            {
                                TakePrisonerAction.Apply(PartyBase.MainParty, h);
                            }
                        }
                        SkipVanillaCaptureLogic = false;
                        BattleHandled = false;
                    }
                    IsMenuOpen = false;
                },
                (List<InquiryElement> selected) => 
                { 
                    if (!fromShiftO)
                    {
                        foreach (var h in allHeroes)
                        {
                            if (h.IsAlive && PartyBase.MainParty != null) 
                            {
                                TakePrisonerAction.Apply(PartyBase.MainParty, h);
                            }
                        }
                        SkipVanillaCaptureLogic = false;
                        BattleHandled = false;
                    }
                    IsMenuOpen = false; 
                }
            ));
        }

        public static void OpenPrisonerManagementUI()
        {
            if (PartyBase.MainParty == null || PartyBase.MainParty.PrisonRoster == null) return;

            List<Hero> heroes = new List<Hero>();
            foreach (TroopRosterElement troop in PartyBase.MainParty.PrisonRoster.GetTroopRoster())
            {
                if (troop.Character != null && troop.Character.IsHero && troop.Character.HeroObject != null && troop.Character.HeroObject.IsAlive)
                {
                    heroes.Add(troop.Character.HeroObject);
                }
            }

            if (heroes.Count == 0)
            {
                IsMenuOpen = false;
                InformationManager.DisplayMessage(new InformationMessage(new TaleWorlds.Localization.TextObject("{=lord_captor_no_prisoners}You do not have any lord prisoners.").ToString()));
                return;
            }

            ShowPrisonerManagementUI(heroes, true);
        }

        public static void ShowApplyToAllUI(List<Hero> allHeroes, bool fromShiftO)
        {
            var elements = new List<InquiryElement>();
            elements.Add(new InquiryElement("execute_all", new TaleWorlds.Localization.TextObject("{=lord_captor_apply_execute_all}Execute All").ToString(), null, true, ""));
            elements.Add(new InquiryElement("release_all", new TaleWorlds.Localization.TextObject("{=lord_captor_apply_release_all}Release All").ToString(), null, true, ""));
            if (!fromShiftO) elements.Add(new InquiryElement("take_all", new TaleWorlds.Localization.TextObject("{=lord_captor_apply_take_all}Take All Prisoner").ToString(), null, true, ""));
            elements.Add(new InquiryElement("back", new TaleWorlds.Localization.TextObject("{=lord_captor_cancel}Cancel").ToString(), null, true, ""));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TaleWorlds.Localization.TextObject("{=lord_captor_apply_to_all}--- Apply To All ---").ToString(),
                new TaleWorlds.Localization.TextObject("{=lord_captor_manage_desc}").SetTextVariable("COUNT", allHeroes.Count).ToString(),
                elements,
                true,
                1,
                1,
                new TaleWorlds.Localization.TextObject("{=lord_captor_continue}Continue").ToString(),
                "",
                (List<InquiryElement> selected) =>
                {
                    string choice = selected[0].Identifier as string;
                    if (choice == "execute_all")
                    {
                        foreach(var h in allHeroes) 
                        {
                            if (h.IsAlive) 
                            {
                                if (!fromShiftO) TakePrisonerAction.Apply(PartyBase.MainParty, h);
                                KillCharacterAction.ApplyByExecution(h, Hero.MainHero, true, true);
                            }
                        }
                        if (!fromShiftO) { SkipVanillaCaptureLogic = false; BattleHandled = false; }
                        IsMenuOpen = false;
                    }
                    else if (choice == "release_all")
                    {
                        foreach(var h in allHeroes) 
                        {
                            if (h.IsAlive) 
                            {
                                if (fromShiftO)
                                {
                                    if (PartyBase.MainParty != null && PartyBase.MainParty.PrisonRoster != null)
                                        PartyBase.MainParty.PrisonRoster.RemoveTroop(h.CharacterObject, 1);
                                }
                                else
                                {
                                    TakePrisonerAction.Apply(PartyBase.MainParty, h);
                                }
                                if (!fromShiftO) 
                                {
                                    EndCaptivityAction.ApplyByReleasedAfterBattle(h);
                                }
                                else
                                {
                                    EndCaptivityAction.ApplyByReleasedByChoice(h, Hero.MainHero);
                                }
                            }
                        }
                        if (!fromShiftO) { SkipVanillaCaptureLogic = false; BattleHandled = false; }
                        IsMenuOpen = false;
                    }
                    else if (choice == "take_all")
                    {
                        if (!fromShiftO)
                        {
                            foreach(var h in allHeroes) if (h.IsAlive && PartyBase.MainParty != null) TakePrisonerAction.Apply(PartyBase.MainParty, h);
                            SkipVanillaCaptureLogic = false;
                            BattleHandled = false;
                        }
                        IsMenuOpen = false;
                    }
                    else if (choice == "back")
                    {
                        ShowPrisonerManagementUI(allHeroes, fromShiftO);
                    }
                },
                (List<InquiryElement> selected) => 
                { 
                    if (!fromShiftO)
                    {
                        foreach(var h in allHeroes) if (h.IsAlive && PartyBase.MainParty != null) TakePrisonerAction.Apply(PartyBase.MainParty, h);
                        SkipVanillaCaptureLogic = false;
                        BattleHandled = false;
                    }
                    IsMenuOpen = false; 
                }
            ));
        }


    }
}
