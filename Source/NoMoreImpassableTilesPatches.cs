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
            if (hilliness == Hilliness.Impassable) __result = NoMoreImpassibleTilesSettings.Instance.MovementDifficulty;
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

                // remove code that actually makes tiles impassable
                int startIndex = -1, endIndex = -1;
                int stage = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (stage == 0 && list[i].opcode == OpCodes.Ldloc_0)
                    {
                        startIndex = i;
                        stage++;
                    }
                    else if ((stage == 1 || stage == 2 || stage == 5) && list[i].opcode == OpCodes.Ldfld) stage++;
                    else if (stage == 3 && list[i].opcode == OpCodes.Brtrue_S) stage++;
                    else if (stage == 4 && list[i].opcode == OpCodes.Ldloc_0) stage++;
                    else if (stage == 6 && list[i].opcode == OpCodes.Ldc_I4_5) stage++;
                    else if (stage == 7 && list[i].opcode == OpCodes.Bne_Un_S) stage++;
                    else if ((stage == 8 || stage == 10) && list[i].opcode == OpCodes.Ldarg_3) stage++;
                    else if (stage == 9 && list[i].opcode == OpCodes.Brfalse_S) stage++;
                    else if (stage == 11 && list[i].opcode == OpCodes.Ldstr) stage++;
                    else if ((stage == 12 || stage == 13) && list[i].opcode == OpCodes.Call) stage++;
                    else if (stage == 14 && list[i].opcode == OpCodes.Callvirt) stage++;
                    else if (stage == 15 && list[i].opcode == OpCodes.Pop) stage++;
                    else if (stage == 16 && list[i].opcode == OpCodes.Ldc_R4) stage++;
                    else if (stage == 17 && list[i].opcode == OpCodes.Ret)
                    {
                        endIndex = i + 1; // all of the code needs to be removed, so must include + 1 so that ret is deleted as well
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

                    Log.Message("[NoMoreImpassableTiles] Reverse patching WorldPathGrid.CalculatedMovementDifficultyAt(): removed impassable tile movement blocker");
                    if (NoMoreImpassibleTilesSettings.Instance.Debug)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var code in list)
                            sb.Append($"({code.labels.Join(x => x.GetHashCode().ToString())}) {code.opcode} {(code.operand is Label lb ? lb.GetHashCode() : code.operand)}\n");
                        Log.Message("[NoMoreImpassableTiles] IL Code:\n" + sb);
                    }
                }

                // replace movement offset with settings value
                startIndex = -1;
                endIndex = -1;
                stage = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (stage == 0 && list[i].opcode == OpCodes.Ldloc_0)
                    {
                        startIndex = i;
                        stage++;
                    }
                    else if (stage == 1 && list[i].opcode == OpCodes.Ldfld) stage++;
                    else if (stage == 2 && list[i].opcode == OpCodes.Call) stage++;
                    else if (stage == 3 && list[i].opcode == OpCodes.Stloc_1)
                    {
                        endIndex = i; // we need stloc.1, so this will be excluded
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
                    // patching code in
                    list.InsertRange(startIndex, new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(NoMoreImpassibleTilesSettings), nameof(NoMoreImpassibleTilesSettings.Instance))),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(NoMoreImpassibleTilesSettings), nameof(NoMoreImpassibleTilesSettings.MovementDifficulty))),
                        new CodeInstruction(OpCodes.Ldind_R4)
                    });
                    list[startIndex].WithLabels(labels);

                    Log.Message("[NoMoreImpassableTiles] Reverse patching WorldPathGrid.CalculatedMovementDifficultyAt(): replaced movement offset for impassable tiles");
                    if (NoMoreImpassibleTilesSettings.Instance.Debug)
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
            return 0f;
        }

        static bool Prefix(ref float __result, int tile, bool perceivedStatic, int? ticksAbs, StringBuilder explanation)
        {
            Tile tile2 = Find.WorldGrid[tile];
            if (NoMoreImpassibleTilesSettings.Instance.OverrideWorldPathfinding
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
                    if (NoMoreImpassibleTilesSettings.Instance.Debug)
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

        static bool Prefix(ref bool __result, int tile, StringBuilder reason)
        {
            Tile tile2 = Find.WorldGrid[tile];
            if (NoMoreImpassibleTilesSettings.Instance.AllowImpassableSettlement && tile2.hilliness == Hilliness.Impassable)
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
            if (NoMoreImpassibleTilesSettings.Instance.MiningSiteAllowImpassable)
                __result = Find.WorldGrid[tile].hilliness >= Hilliness.LargeHills;
        }
    }
}
