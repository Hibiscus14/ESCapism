using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
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
        public const string PluginVersion = "1.0.0";

        private static List<RoR2.UI.HGButton> CancelButtons = new List<RoR2.UI.HGButton>();

        public void Awake()
        {
            IL.RoR2.UI.MPEventSystem.Update += HandleEventSystemUpdate;

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
                    if (button.name.Equals("CancelButton"))
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


