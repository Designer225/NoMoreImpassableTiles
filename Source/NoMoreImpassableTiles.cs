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
                "NoMoreImpassableTiles.OverrideWorldPathfinding".Translate(),
                ref settings.OverrideWorldPathfinding,
                "NoMoreImpassableTiles.OverrideWorldPathfinding.Tooltip".Translate());
            if (settings.OverrideWorldPathfinding)
                listingStandard.TextFieldNumericLabeled(
                    "NoMoreImpassableTiles.MovementDifficulty".Translate(),
                    ref settings.MovementDifficulty, ref movementDifficultyBuffer, 0.1f, 1000f);
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.AllowImpassableSettlement".Translate(),
                ref settings.AllowImpassableSettlement,
                "NoMoreImpassableTiles.AllowImpassableSettlement.Tooltip".Translate());
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.MiningSiteAllowImpassable".Translate(),
                ref settings.MiningSiteAllowImpassable,
                "NoMoreImpassableTiles.MiningSiteAllowImpassable.Tooltip".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled(
                "NoMoreImpassableTiles.Debug".Translate(),
                ref settings.Debug,
                "NoMoreImpassableTiles.Debug.Tooltip".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "NoMoreImpassableTiles".Translate();
        }
    }
}
