using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace LordCaptor
{
    public class LordCaptorSubModule : MBSubModuleBase
    {
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
            // We no longer need to register a CampaignBehavior because the 
            // logic is fully handled by Harmony patches and OnApplicationTick.
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            
            if (Campaign.Current != null && Hero.MainHero != null && TaleWorlds.CampaignSystem.Party.PartyBase.MainParty != null && TaleWorlds.CampaignSystem.Party.PartyBase.MainParty.MobileParty != null)
            {
                if (Input.IsKeyDown(InputKey.LeftShift) && Input.IsKeyPressed(InputKey.O))
                {
                    LordCaptorManager.Log("Shift+O pressed! Opening UI.");
                    LordCaptorManager.OpenPrisonerManagementUI();
                }
            }
        }
    }
}
