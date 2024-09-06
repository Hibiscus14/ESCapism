using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2.UI;
using System;
using System.Collections.Generic;

namespace ESCapism
{
    [BepInDependency(ItemAPI.PluginGUID)]

    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class ESCapismPlugin : BaseUnityPlugin
    {

        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Hibiscus";
        public const string PluginName = "ESCapism";
        public const string PluginVersion = "1.5.0";


        public static ConfigEntry<bool> EnabledInGame { get; set; }
        public static ConfigEntry<bool> EnabledInMenu { get; set; }

        private static List<RoR2.UI.HGButton> CancelButtons = new List<RoR2.UI.HGButton>();

        public void Awake()
        {
            CreateConfig();

            if (Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                SetupRiskOfOptions();
            }
            
            IL.RoR2.UI.MPEventSystem.Update += HandleEventSystemUpdate;
        }

        private void CreateConfig()
        {
            EnabledInGame = Config.Bind<bool>(
            "General",
            "Enabled in game",
                    true,
            "If enabled, you will be able to close windows by pressing ESCAPE in game"
            );

            EnabledInMenu = Config.Bind<bool>(
            "General",
            "Enabled in menu",
                    true,
            "If enabled, you will be able to close windows by pressing ESCAPE in menu"
            );
        }

        private void SetupRiskOfOptions()
        {
            ModSettingsManager.AddOption(new CheckBoxOption(EnabledInGame));
            ModSettingsManager.AddOption(new CheckBoxOption(EnabledInMenu));
        }

        private void HandleEventSystemUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(x => x.MatchCall<RoR2.Console>("get_instance"));
            var label = c.DefineLabel();
            c.MarkLabel(label);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<MPEventSystem>>((eventSystem) =>
            {
                foreach (var button in FindObjectsOfType<RoR2.UI.HGButton>())
                {
                    if ((button.name.Equals("CancelButton")&& EnabledInGame.Value == true) || (button.name.StartsWith("NakedButton") && EnabledInMenu.Value == true))
                    {
                        CancelButtons.Add(button);
                    }
                }

                if (CancelButtons.Count > 0)
                {
                    CloseMenu();
                }

                else
                {
                    RoR2.Console.instance.SubmitCmd(null, "pause");
                }
            });
            c.Emit(OpCodes.Ret);
            c.GotoPrev(x => x.MatchBrfalse(out _));
            c.Next.Operand = label;
            c.Next.OpCode = OpCodes.Brfalse;
        }


        public static void CloseMenu()
        {
            while (CancelButtons.Count != 0)
            {
                if (CancelButtons[0] is not null)
                {
                    CancelButtons[0].InvokeClick();
                }
                CancelButtons.RemoveAt(0);
            }
        }

    }

}


