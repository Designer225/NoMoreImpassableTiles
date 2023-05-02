using System;
using UnityEngine;
using Verse;

namespace NoMoreImpassableTiles
{
    public sealed class NoMoreImpassableTiles : Mod
    {
        NoMoreImpassibleTilesSettings settings;

        private string movementDifficultyBuffer;

        public NoMoreImpassableTiles(ModContentPack content) : base(content)
        {
            settings = GetSettings<NoMoreImpassibleTilesSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled(
                "NoMoreImpassableTiles.overrideWorldPathfinding".Translate(),
                ref settings.OverrideWorldPathfinding,
                "NoMoreImpassableTiles.overrideWorldPathfinding.tooltip".Translate());
            if (settings.OverrideWorldPathfinding)
                listingStandard.TextFieldNumericLabeled(
                    "NoMoreImpassableTiles.movementDifficulty".Translate(),
                    ref settings.MovementDifficulty, ref movementDifficultyBuffer, 0.1f, 1000f);
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.allowImpassableSettlement".Translate(),
                ref settings.AllowImpassableSettlement,
                "NoMoreImpassableTiles.allowImpassableSettlement.tooltip".Translate());
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.miningSiteAllowImpassable".Translate(),
                ref settings.MiningSiteAllowImpassable,
                "NoMoreImpassableTiles.miningSiteAllowImpassable.tooltip".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled(
                "NoMoreImpassableTiles.debug".Translate(),
                ref settings.Debug,
                "NoMoreImpassableTiles.debug.tooltip".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "NoMoreImpassableTiles".Translate();
        }
    }
}
