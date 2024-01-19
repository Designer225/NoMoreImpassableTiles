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
                __result = computedDifficulty + WorldPathGrid.GetCurrentWinterMovementDifficultyOffset(tile, new int?(ticksAbs ?? GenTicks.TicksAbs), explanation);
            }
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
                int stage = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (stage == 0 && list[i].opcode == OpCodes.Ldloc_0)
                    {
                        startIndex = i;
                        stage++;
                    }
                    else if (stage == 1 && list[i].opcode == OpCodes.Ldfld) stage++;
                    else if (stage == 2 && list[i].opcode == OpCodes.Ldc_I4_5) stage++;
                    else if (stage == 3 && list[i].opcode == OpCodes.Bne_Un_S) stage++;
                    else if ((stage == 4 || stage == 6) && list[i].opcode == OpCodes.Ldarg_1) stage++;
                    else if (stage == 5 && list[i].opcode == OpCodes.Brfalse_S) stage++;
                    else if (stage == 7 && list[i].opcode == OpCodes.Ldstr) stage++;
                    else if ((stage == 8 || stage == 9) && list[i].opcode == OpCodes.Call) stage++;
                    else if (stage == 10 && list[i].opcode == OpCodes.Callvirt) stage++;
                    else if (stage == 11 && list[i].opcode == OpCodes.Pop) stage++;
                    else if (stage == 12 && list[i].opcode == OpCodes.Ldc_I4_0) stage++;
                    else if (stage == 13 && list[i].opcode == OpCodes.Ret)
                    {
                        endIndex = i + 1;
                        break;
                    }
                    else
                    {
                        startIndex = -1;
                        stage = 0;
                    }
                }

                if (startIndex > -1 && endIndex > -1)
                {
                    var labels = list[startIndex].ExtractLabels();
                    list.RemoveRange(startIndex, endIndex - startIndex);
                    list[startIndex].WithLabels(labels);

                    Log.Message("[NoMoreImpassableTiles] Reverse patching TileFinder.IsValidTileForNewSettlement(): removed impassable tile settlement blocker");
                    if (NoMoreImpassableTilesSettings.Instance.Debug)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var code in list)
                            sb.Append($"({code.labels.Join(x => x.GetHashCode().ToString())}) {code.opcode} {(code.operand is Label lb ? lb.GetHashCode() : code.operand)}\n");
                        Log.Message("[NoMoreImpassableTiles] IL Code:\n" + sb);
                    }
                }

                return list.AsEnumerable();
            }

            _ = Transpiler(null);
            return false;
        }

        static void Prepare() { Log.Message("[NoMoreImpassableTiles] Patching TileFinder.IsValidTileForNewSettlement() with a postfix"); }

        [HarmonyPriority(Priority.First)]
        static void Postfix(ref bool __result, int tile, StringBuilder reason)
        {
            Tile tile2 = Find.WorldGrid[tile];
            if (NoMoreImpassableTilesSettings.Instance.AllowImpassableSettlement && tile2.hilliness == Hilliness.Impassable)
            {
                __result = __result ? __result : OriginalMethod(tile, reason);
            }
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
