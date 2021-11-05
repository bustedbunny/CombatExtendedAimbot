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



namespace CombatExtendedAimbot
{
    class CEASettings
    {
        public class CombatExtendedAimbotSettings : ModSettings
        {
            public bool disableAI;
            public override void ExposeData()
            {
                Scribe_Values.Look(ref disableAI, "disableAI");
                base.ExposeData();
            }
        }
        public class CombatExtendedAimbotMod : Mod
        {
            readonly CombatExtendedAimbotSettings settings;
            public CombatExtendedAimbotMod(ModContentPack content) : base(content)
            {
                this.settings = GetSettings<CombatExtendedAimbotSettings>();
            }
            public override void DoSettingsWindowContents(Rect inRect)
            {
                Listing_Standard listingStandard = new Listing_Standard();
                listingStandard.Begin(inRect);
                listingStandard.CheckboxLabeled("Disable Aimbot for AI pawns", ref settings.disableAI, "Enables base auto switching of CE and disables Aimbot on all AI pawns.");
                listingStandard.End();
                base.DoSettingsWindowContents(inRect);
            }
            public override string SettingsCategory()
            {
                return "Combat Extended Aimbot";
            }
        }
    }
}
