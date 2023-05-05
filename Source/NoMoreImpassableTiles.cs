using System;
using UnityEngine;
using Verse;

namespace NoMoreImpassableTiles
{
    public sealed class NoMoreImpassableTiles : Mod
    {

        private string movementDifficultyBuffer;

        public NoMoreImpassableTiles(ModContentPack content) : base(content) { }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled(
                "NoMoreImpassableTiles.OverrideWorldPathfinding".Translate(),
                ref NoMoreImpassableTilesSettings.Instance.OverrideWorldPathfinding,
                "NoMoreImpassableTiles.OverrideWorldPathfinding.Tooltip".Translate());
            if (NoMoreImpassableTilesSettings.Instance.OverrideWorldPathfinding)
                listingStandard.TextFieldNumericLabeled(
                    "NoMoreImpassableTiles.MovementDifficulty".Translate(),
                    ref NoMoreImpassableTilesSettings.Instance.MovementDifficulty, ref movementDifficultyBuffer, 0.1f, 1000f);
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.AllowImpassableSettlement".Translate(),
                ref NoMoreImpassableTilesSettings.Instance.AllowImpassableSettlement,
                "NoMoreImpassableTiles.AllowImpassableSettlement.Tooltip".Translate());
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.MiningSiteAllowImpassable".Translate(),
                ref NoMoreImpassableTilesSettings.Instance.MiningSiteAllowImpassable,
                "NoMoreImpassableTiles.MiningSiteAllowImpassable.Tooltip".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled(
                "NoMoreImpassableTiles.Debug".Translate(),
                ref NoMoreImpassableTilesSettings.Instance.Debug,
                "NoMoreImpassableTiles.Debug.Tooltip".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "NoMoreImpassableTiles".Translate();
    }
}
