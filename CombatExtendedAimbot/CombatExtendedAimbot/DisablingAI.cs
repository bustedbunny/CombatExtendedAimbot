using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;
using CombatExtended;
using UnityEngine;
using CombatExtended.AI;
using static CombatExtendedAimbot.CEASettings;

namespace CombatExtendedAimbot
{
    class DisablingAI
    {
        [HarmonyPatch(typeof(CompFireModes), "CurrentFireMode", MethodType.Getter)]
        class DisableAI
        {
            public static void Prefix(CompFireModes __instance)
            {
                if (!LoadedModManager.GetMod<CombatExtendedAimbotMod>().GetSettings<CombatExtendedAimbotSettings>().disableAI)
                {
                    __instance.Props.aiUseBurstMode = false;
                }

            }
        }

        [HarmonyPatch(typeof(CompFireModes), "useAIModes", MethodType.Getter)]
        class DisableAIPrivate
        {
            public static bool Prefix(ref bool __result)
            {
                if (!LoadedModManager.GetMod<CombatExtendedAimbotMod>().GetSettings<CombatExtendedAimbotSettings>().disableAI)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(AI_Utility), "TrySetFireMode")]
        class DisablingAITrySetFireMode
        {
            public static bool Prefix()
            {
                if (!LoadedModManager.GetMod<CombatExtendedAimbotMod>().GetSettings<CombatExtendedAimbotSettings>().disableAI)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(AI_Utility), "TrySetAimMode")]
        class DisablingAITrySetAimMode
        {
            public static bool Prefix()
            {
                if (!LoadedModManager.GetMod<CombatExtendedAimbotMod>().GetSettings<CombatExtendedAimbotSettings>().disableAI)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
