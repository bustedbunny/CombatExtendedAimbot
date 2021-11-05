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
    class HarmonyPatches
    {
        [StaticConstructorOnStartup]
        public static class StartUp
        {
            static StartUp()
            {
                var harmony = new Harmony("CombatExtendedAimbot.patch");
                harmony.PatchAll();
            }
        }

        

        [HarmonyPatch(typeof(CompFireModes), "ToggleFireMode")]
        class DisableFromFire
        {
            public static void Postfix(CompFireModes __instance)
            {
                if (__instance.CasterPawn != null && __instance.CasterPawn.IsColonist)
                {
                    Pawn_AimbotTracker.DisableAimbot(__instance.CasterPawn);
                }

            }
        }

        [HarmonyPatch(typeof(CompFireModes), "ToggleAimMode")]
        class DisableFromAim
        {
            public static void Postfix(CompFireModes __instance)
            {
                if (__instance.CasterPawn != null && __instance.CasterPawn.IsColonist)
                {
                    Pawn_AimbotTracker.DisableAimbot(__instance.CasterPawn);
                }
            }
        }

        [HarmonyPatch(typeof(CompFireModes), "GenerateGizmos")]
        class TurnAimbotBackOn
        {
            public static IEnumerable<Command> Postfix(IEnumerable<Command> __result, CompFireModes __instance)
            {
                foreach (Command result in __result)
                {
                    yield return result;
                }

                Pawn pawn = __instance.CasterPawn;
                if (!pawn?.IsColonist ?? true)
                {
                    yield break;
                }
                if (!Pawn_AimbotTracker.Get(pawn).AimbotStatus)
                {
                    yield return new Command_Action
                    {
                        action = delegate () { Pawn_AimbotTracker.EnableAimbot(pawn); },
                        defaultLabel = ("CEA_TurnAimbotOn").Translate(),
                        defaultDesc = ("CEA_TurnAimbotOnDesc").Translate(),
                        icon = ContentFinder<Texture2D>.Get("EnableAimbotGizmo")
                    };
                }
            }
        }

        [HarmonyPatch(typeof(Verb_ShootCE), "CanHitTargetFrom")]
        class CanHitTargetFromPatch
        {
            public static void Postfix(Verb_ShootCE __instance, bool __result, IntVec3 root, LocalTargetInfo targ)
            {
                if (__instance.ShooterPawn == null)
                {
                    return;
                }
                if (!__result || !__instance.ShooterPawn.RaceProps.Humanlike || !Pawn_AimbotTracker.Get(__instance.ShooterPawn).AimbotStatus)
                {
                    return;
                }
                if (LoadedModManager.GetMod<CombatExtendedAimbotMod>().GetSettings<CombatExtendedAimbotSettings>().disableAI && __instance.ShooterPawn.Faction != Faction.OfPlayer)
                {
                    return;
                }
                CompFireModes firemods = __instance.ShooterPawn?.equipment?.Primary?.TryGetComp<CompFireModes>();
                if (firemods != null)
                {
                    int distance = IntVec3Utility.ManhattanDistanceFlat(root, targ.Cell);
                    if (firemods.AvailableFireModes.Count > 1)
                    {
                        HelperClass.SwitchFireMode(distance, firemods, __instance.VerbPropsCE);
                    }
                    if (firemods.AvailableAimModes.Count > 2)
                    {
                        HelperClass.SwitchAimMode(distance, firemods, __instance.VerbPropsCE);
                    }
                }
            }
        }

        public static class HelperClass
        {
            public static void SwitchFireMode(int distance, CompFireModes firemods, VerbPropertiesCE verb)
            {
                bool lmg = verb.burstShotCount > 6;
                if (distance > 25)
                {
                    if (firemods.AvailableFireModes.Contains(FireMode.SingleFire) && !lmg)
                    {
                        firemods.CurrentFireMode = FireMode.SingleFire;
                    }
                    else if (firemods.AvailableFireModes.Contains(FireMode.BurstFire))
                    {
                        firemods.CurrentFireMode = FireMode.BurstFire;
                    }

                }
                else if (distance > 10)
                {
                    if (firemods.AvailableFireModes.Contains(FireMode.BurstFire) && !lmg)
                    {
                        firemods.CurrentFireMode = FireMode.BurstFire;
                    }
                    else if (firemods.AvailableFireModes.Contains(FireMode.AutoFire))
                    {
                        firemods.CurrentFireMode = FireMode.AutoFire;
                    }
                }
                else
                {
                    if (firemods.AvailableFireModes.Contains(FireMode.AutoFire))
                    {
                        firemods.CurrentFireMode = FireMode.AutoFire;
                    }
                }
            }
            public static void SwitchAimMode(int distance, CompFireModes firemods, VerbPropertiesCE verb)
            {
                if (firemods.CurrentFireMode == FireMode.SingleFire)
                {
                    firemods.CurrentAimMode = distance > 10 ? AimMode.AimedShot : distance > 5 ? AimMode.Snapshot : AimMode.SuppressFire;
                }
                else if (firemods.CurrentFireMode == FireMode.BurstFire)
                {
                    if (firemods.Props.aimedBurstShotCount == 3)
                    {
                        firemods.CurrentAimMode = distance > 18 ? AimMode.AimedShot : distance > 10 ? AimMode.Snapshot : AimMode.SuppressFire;
                    }
                    else
                    {
                        firemods.CurrentAimMode = distance > 10 ? AimMode.Snapshot : AimMode.SuppressFire;
                    }
                }
                else
                {
                    if (verb.burstShotCount < 7)
                    {
                        firemods.CurrentAimMode = distance > 10 ? AimMode.Snapshot : AimMode.SuppressFire;
                    }
                    else
                    {
                        firemods.CurrentAimMode = distance > 18 ? AimMode.Snapshot : AimMode.SuppressFire;
                    }
                }
            }
        }
    }
}
