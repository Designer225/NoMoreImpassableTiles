using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace NoMoreImpassableTiles
{
    [StaticConstructorOnStartup]
    internal class NoMoreImpassableTilesPatches
    {
        static NoMoreImpassableTilesPatches()
        {
            new Harmony("Designer225.NoMoreImpassableTiles").PatchAll();
        }
    }

    [HarmonyPatch(typeof(WorldPathGrid), nameof(WorldPathGrid.CalculatedMovementDifficultyAt))]
    internal static class WorldPathGrid_CalculatedMovementDifficultyAtPatch
    {
        static void Prepare() { Log.Message("[NoMoreImpassableTiles] Patching WorldPathGrid.CalculatedMovementDifficultyAt() with a postfix"); }

        [HarmonyPriority(Priority.First)]
        static void Postfix(ref float __result, int tile, int? ticksAbs, StringBuilder explanation)
        {
            Tile tile2 = Find.WorldGrid[tile];
            if (NoMoreImpassableTilesSettings.Instance.OverrideWorldPathfinding
                && (tile2.biome.impassable || tile2.hilliness == Hilliness.Impassable) && Mathf.Approximately(__result, 1000f))
            {
                if (explanation != null)
                {
                    explanation.Clear();
                    explanation.AppendLine();
                }
                explanation?.Append(tile2.biome.LabelCap + ": " + tile2.biome.movementDifficulty.ToStringWithSign("0.#"));
                float movementDifficulty = NoMoreImpassableTilesSettings.Instance.MovementDifficulty;
                float computedDifficulty = tile2.biome.movementDifficulty + movementDifficulty;
                if (explanation != null && movementDifficulty != 0f)
                {
                    explanation.AppendLine();
                    explanation.Append(tile2.hilliness.GetLabelCap() + ": " + movementDifficulty.ToStringWithSign("0.#"));
                }
                __result = computedDifficulty + WorldPathGrid.GetCurrentWinterMovementDifficultyOffset(tile, ticksAbs ?? GenTicks.TicksAbs, explanation);
            }
        }
    }

    [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.IsValidTileForNewSettlement))]
    internal static class TileFinder_IsValidTileForNewSettlement_Patch
    {
        static void Prepare() { Log.Message("[NoMoreImpassableTiles] Patching TileFinder.IsValidTileForNewSettlement() with a postfix"); }

        [HarmonyPriority(Priority.First)]
        static void Postfix(ref bool __result, int tile, StringBuilder reason)
        {
            if (__result) return;
            
            var tile1 = Find.WorldGrid[tile];
            if (!NoMoreImpassableTilesSettings.Instance.AllowImpassableSettlement ||
                tile1.hilliness != Hilliness.Impassable) return;
            reason?.Clear();
            // from decompiled source code of TileFinder.IsValidTileForNewSettlement
            if (!tile1.biome.canBuildBase)
            {
                reason?.Append("CannotLandBiome".Translate(tile1.biome.LabelCap));
                return; // already false
            }
            if (!tile1.biome.implemented)
            {
                reason?.Append("BiomeNotImplemented".Translate() + ": " + tile1.biome.LabelCap);
                return; // already false
            }
            var settlement = Find.WorldObjects.SettlementBaseAt(tile);
            if (settlement != null)
            {
                if (reason != null)
                {
                    if (settlement.Faction == null)
                        reason.Append("TileOccupied".Translate());
                    else if (settlement.Faction == Faction.OfPlayer)
                        reason.Append("YourBaseAlreadyThere".Translate());
                    else
                        reason.Append("BaseAlreadyThere".Translate((NamedArgument) settlement.Faction.Name));
                }
                return; // already false
            }
            if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tile))
            {
                reason?.Append("FactionBaseAdjacent".Translate());
                return; // already false
            }

            if (!Find.WorldObjects.AnyMapParentAt(tile) && Current.Game.FindMap(tile) == null &&
                !Find.WorldObjects.AnyWorldObjectOfDefAt(WorldObjectDefOf.AbandonedSettlement, tile))
            {
                __result = true;
                return;
            }
            reason?.Append("TileOccupied".Translate());
            // already false
        }
    }

    [HarmonyPatch(typeof(SitePartWorker_WorkSite_Mining), nameof(SitePartWorker_WorkSite_Mining.CanSpawnOn))]
    internal static class SitePartWorker_WorkSite_Mining_CanSpawnOnPatch
    {
        static void Prepare() { Log.Message("[NoMoreImpassableTiles] Patching SitePartWorker_WorkSite_Mining.CanSpawnOn() with a postfix"); }

        static void Postfix(ref bool __result, int tile)
        {
            if (NoMoreImpassableTilesSettings.Instance.MiningSiteAllowImpassable)
                __result = Find.WorldGrid[tile].hilliness >= Hilliness.LargeHills;
        }
    }
}
