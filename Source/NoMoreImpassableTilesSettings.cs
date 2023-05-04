using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace NoMoreImpassableTiles
{
    public sealed class NoMoreImpassableTilesSettings : ModSettings
    {
        private const bool DefaultOverrideWorldPathfinding = true;
        private const float DefaultMovementDifficulty = 5f;
        private const bool DefaultAllowImpassableSettlement = true;
        private const bool DefaultMiningSiteAllowImpassable = true;
        private const bool DefaultDebug = false;

        private static NoMoreImpassableTilesSettings instance;
        public static NoMoreImpassableTilesSettings Instance
        {
            get
            {
                if (instance == default) instance = LoadedModManager.GetMod<NoMoreImpassableTiles>().GetSettings<NoMoreImpassableTilesSettings>();
                return instance;
            }
        }

        private bool m_overrideWorldPathfinding = DefaultOverrideWorldPathfinding;
        public ref bool OverrideWorldPathfinding => ref m_overrideWorldPathfinding;

        private float m_movementDifficulty = DefaultMovementDifficulty;
        public ref float MovementDifficulty => ref m_movementDifficulty;

        private bool m_allowImpassableSettlement = DefaultAllowImpassableSettlement;
        public ref bool AllowImpassableSettlement => ref m_allowImpassableSettlement;

        private bool m_miningSiteAllowImpassable = DefaultMiningSiteAllowImpassable;
        public ref bool MiningSiteAllowImpassable => ref m_miningSiteAllowImpassable;

        private bool m_debug = DefaultDebug;
        public ref bool Debug => ref m_debug;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref m_overrideWorldPathfinding, "OverrideWorldPathfinding", DefaultOverrideWorldPathfinding);
            Scribe_Values.Look(ref m_movementDifficulty, "MovementDifficulty", DefaultMovementDifficulty);
            Scribe_Values.Look(ref m_allowImpassableSettlement, "AllowImpassableSettlement", DefaultAllowImpassableSettlement);
            Scribe_Values.Look(ref m_miningSiteAllowImpassable, "MiningSiteAllowImpassable", DefaultMiningSiteAllowImpassable);
            Scribe_Values.Look(ref m_debug, "Debug", DefaultDebug);
            base.ExposeData();
        }
    }
}
