using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace LordCaptor
{
    public class LordCaptorSubModule : MBSubModuleBase
    {
        private bool _wasMenuOpen = false;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                LordCaptorManager.Log("OnSubModuleLoad called! Attempting PatchAll...");
                new Harmony("lordcaptor.bannerlord").PatchAll();
                LordCaptorManager.Log("PatchAll SUCCESS!");
            }
            catch (System.Exception ex)
            {
                LordCaptorManager.Log("PatchAll FAILED: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            
            try
            {
                if (_wasMenuOpen && !LordCaptorManager.IsMenuOpen) LordCaptorManager.SyncPartyScreen();
                _wasMenuOpen = LordCaptorManager.IsMenuOpen;

                if (TaleWorlds.Core.Game.Current != null && TaleWorlds.Core.Game.Current.GameStateManager != null)
                {
                    var activeState = TaleWorlds.Core.Game.Current.GameStateManager.ActiveState;
                    if (activeState is TaleWorlds.CampaignSystem.GameState.MapState || activeState is TaleWorlds.CampaignSystem.GameState.PartyState)
                    {
                        if (Campaign.Current != null && Hero.MainHero != null && TaleWorlds.CampaignSystem.Party.PartyBase.MainParty != null)
                        {
                            if (!LordCaptorManager.IsMenuOpen && Input.IsKeyDown(InputKey.LeftShift) && Input.IsKeyPressed(InputKey.O))
                            {
                                LordCaptorManager.IsMenuOpen = true;
                                LordCaptorManager.Log("Shift+O pressed! Opening UI.");
                                LordCaptorManager.OpenPrisonerManagementUI();
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
