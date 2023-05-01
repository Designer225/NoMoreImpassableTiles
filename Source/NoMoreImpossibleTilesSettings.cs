using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace NoMoreImpassableTiles
{
    public sealed class NoMoreImpossibleTilesSettings : ModSettings
    {
        private static NoMoreImpossibleTilesSettings instance;
        
        public static NoMoreImpossibleTilesSettings Instance
        {
            get
            {
                instance = LoadedModManager.GetMod<NoMoreImpassableTiles>().GetSettings<NoMoreImpossibleTilesSettings>();
                return instance;
            }
        }

        public bool overrideWorldPathfinding;

        public float movementDifficulty;

        public bool allowImpassableSettlement;

        public bool miningSiteAllowImpassable;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref overrideWorldPathfinding, "overrideWorldPathfinding", true);
            Scribe_Values.Look(ref movementDifficulty, "movementDifficulty", 4f);
            Scribe_Values.Look(ref allowImpassableSettlement, "allowImpassableSettlement", true);
            Scribe_Values.Look(ref miningSiteAllowImpassable, "miningSiteAllowImpassable", true);
            base.ExposeData();
        }
    }
}
