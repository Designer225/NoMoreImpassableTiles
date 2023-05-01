using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace NoMoreImpassableTiles
{
    [StaticConstructorOnStartup]
    internal class NoMoreImpassableTilesPatches
    {
        static NoMoreImpassableTilesPatches()
        {
            new Harmony("designer225.nomoreimpassabletiles").PatchAll();
        }
    }

    [HarmonyPatch(typeof(WorldPathGrid), "HillinessMovementDifficultyOffset")]
    internal static class WorldPathGrid_HillinessMovementDifficultyOffsetPatch
    {
        static void Postfix(ref float __result, Hilliness hilliness)
        {
            if (hilliness == Hilliness.Impassable) __result = NoMoreImpossibleTilesSettings.Instance.movementDifficulty;
        }
    }

    [HarmonyPatch(typeof(WorldPathGrid), nameof(WorldPathGrid.CalculatedMovementDifficultyAt))]
    internal static class WorldPathGrid_CalculatedMovementDifficultyAtPatch
    {
        [HarmonyReversePatch]
        static float OriginalMethod(int tile, bool perceivedStatic, int? tickAbs = null, StringBuilder explanation = null)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();

                int startIndex = -1, endIndex = -1;
                for (int i = 0; i < list.Count; i++)
                {
                    if (startIndex > -1)
                    {
                        if (list[i].opcode == OpCodes.Ret)
                        {
                            endIndex = i + 1;
                            break;
                        }
                    }
                    else
                    {
                        if (list[i].opcode == OpCodes.Ldloc_0 && i + 1 < list.Count
                            && list[i + 1].opcode == OpCodes.Ldfld && list[i + 1].operand is FieldInfo field
                            && field == AccessTools.Field(typeof(Tile), nameof(Tile.biome)))
                            startIndex = i;
                    }
                }

                if (startIndex > -1 && endIndex > -1)
                    list.RemoveRange(startIndex, endIndex - startIndex);

                return list.AsEnumerable();
            }

            _ = Transpiler(null);
            return 0f;
        }

        static bool Prefix(ref float __result, int tile, bool perceivedStatic, int? ticksAbs, StringBuilder explanation)
        {
            Tile tile2 = Find.WorldGrid[tile];
            if (NoMoreImpossibleTilesSettings.Instance.overrideWorldPathfinding
                && (tile2.biome.impassable || tile2.hilliness == Hilliness.Impassable))
            {
                __result = OriginalMethod(tile, perceivedStatic, ticksAbs, explanation);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.IsValidTileForNewSettlement))]
    internal static class TileFinder_IsValidTileForNewSettlement_Patch
    {
        [HarmonyReversePatch]
        static bool OriginalMethod(int tile, StringBuilder reason = null)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();

                int startIndex = -1, endIndex = -1;
                for (int i = 0; i < list.Count; i++)
                {
                    if (startIndex > -1)
                    {
                        if (list[i].opcode == OpCodes.Ret)
                        {
                            endIndex = i + 1;
                            break;
                        }
                    }
                    else
                    {
                        if (list[i].opcode == OpCodes.Ldloc_0 && i + 1 < list.Count
                            && list[i + 1].opcode == OpCodes.Ldfld && list[i + 1].operand is FieldInfo field
                            && field == AccessTools.Field(typeof(Tile), nameof(Tile.hilliness)))
                            startIndex = i;
                    }
                }

                if (startIndex > -1 && endIndex > -1)
                    list.RemoveRange(startIndex, endIndex - startIndex);

                return list.AsEnumerable();
            }

            _ = Transpiler(null);
            return false;
        }

        static bool Prefix(ref bool __result, int tile, StringBuilder reason)
        {
            Tile tile2 = Find.WorldGrid[tile];
            if (NoMoreImpossibleTilesSettings.Instance.allowImpassableSettlement && tile2.hilliness == Hilliness.Impassable)
            {
                __result = OriginalMethod(tile, reason);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SitePartWorker_WorkSite_Mining), nameof(SitePartWorker_WorkSite_Mining.CanSpawnOn))]
    internal static class SitePartWorker_WorkSite_Mining_CanSpawnOnPatch
    {
        static void Postfix(ref bool __result, int tile)
        {
            __result = Find.WorldGrid[tile].hilliness >= Hilliness.LargeHills;
        }
    }
}
