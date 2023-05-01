using System;
using UnityEngine;
using Verse;

namespace NoMoreImpassableTiles
{
    public sealed class NoMoreImpassableTiles : Mod
    {
        NoMoreImpossibleTilesSettings settings;

        private string movementDifficultyBuffer;

        public NoMoreImpassableTiles(ModContentPack content) : base(content)
        {
            settings = GetSettings<NoMoreImpossibleTilesSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.overrideWorldPathfinding", ref settings.overrideWorldPathfinding);
            if (settings.overrideWorldPathfinding)
                listingStandard.TextFieldNumericLabeled(
                    "NoMoreImpassableTiles.movementDifficulty", ref settings.movementDifficulty, ref movementDifficultyBuffer,
                    0.1f, 1000f);
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.allowImpassableSettlement", ref settings.allowImpassableSettlement);
            listingStandard.CheckboxLabeled("NoMoreImpassableTiles.miningSiteAllowImpassable", ref settings.miningSiteAllowImpassable);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "NoMoreImpassableTiles".Translate();
        }
    }
}
