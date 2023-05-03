using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace NoMoreImpassableTiles
{
    public sealed class NoMoreImpassibleTilesSettings : ModSettings
    {
        private const bool DefaultOverrideWorldPathfinding = true;
        private const float DefaultMovementDifficulty = 5f;
        private const bool DefaultAllowImpassableSettlement = true;
        private const bool DefaultMiningSiteAllowImpassable = true;
        private const bool DefaultDebug = false;

        private static NoMoreImpassibleTilesSettings m_instance;
        
        public static NoMoreImpassibleTilesSettings Instance
        {
            get
            {
                m_instance = LoadedModManager.GetMod<NoMoreImpassableTiles>().GetSettings<NoMoreImpassibleTilesSettings>();
                return m_instance;
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
            Scribe_Values.Look(ref m_overrideWorldPathfinding, "overrideWorldPathfinding", DefaultOverrideWorldPathfinding);
            Scribe_Values.Look(ref m_movementDifficulty, "movementDifficulty", DefaultMovementDifficulty);
            Scribe_Values.Look(ref m_allowImpassableSettlement, "allowImpassableSettlement", DefaultAllowImpassableSettlement);
            Scribe_Values.Look(ref m_miningSiteAllowImpassable, "miningSiteAllowImpassable", DefaultMiningSiteAllowImpassable);
            Scribe_Values.Look(ref m_debug, "debug", DefaultDebug);
            base.ExposeData();
        }
    }
}
