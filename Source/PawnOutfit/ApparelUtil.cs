﻿using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;
using System;

namespace ChangeDresser
{
    public static class ApparelUtil
    {
        public static List<Apparel> RemoveApparel(Pawn pawn)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin ApparelUtil.RemoveApparel(Pawn: " + pawn.Name.ToStringShort + ")");
            Log.Message("    Remove Apparel:");
#endif
            List<Apparel> wornApparel = new List<Apparel>(pawn.apparel.WornApparel);
            foreach (Apparel a in wornApparel)
            {
#if DRESSER_OUTFIT
                Log.Message("        " + a.Label);
#endif
                pawn.apparel.Remove(a);
            }

            pawn.outfits.forcedHandler.ForcedApparel.Clear();
#if DRESSER_OUTFIT
            Log.Warning("End ApparelUtil.RemoveApparel Removed Count: " + wornApparel.Count);
#endif
            return wornApparel;
        }

        public static void StoreApparelInWorldDresser(List<Apparel> apparel, Pawn pawn)
        {
#if DRESSER_OUTFIT
            Log.Warning("Begin ApparelUtil.StoreApparelInWorldDresser(Pawn: " + pawn.Name.ToStringShort + ")");
            Log.Message("    Store Apparel in World Dressers:");
#endif
            foreach (Apparel a in apparel)
            {
#if DRESSER_OUTFIT
                Log.Message("        " + a.Label);
#endif
                if (!WorldComp.AddApparel(a))
                {
#if DRESSER_OUTFIT
                    Log.Warning("            Unable to place apparel in dresser, dropping to floor");
#endif
                    BuildingUtil.DropThing(a, pawn.Position, pawn.Map, false);
                }
            }
#if DRESSER_OUTFIT
            Log.Warning("End ApparelUtil.StoreApparelInWorldDresser");
#endif
        }

        static readonly MethodInfo optApparelMI =
            typeof(JobGiver_OptimizeApparel).GetMethod("TryGiveJob", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void OptimizeApparel(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.Warning("OptimizeApparel called with a null Pawn.");
                return; // Exit the method if pawn is null to prevent further errors.
            }
            bool hasDressers = WorldComp.HasDressers(pawn.Map);
            if (hasDressers)
            {
#if DRESSER_OUTFIT
            Log.Warning("Begin OptimizeApparelUtil.OptimizeApparel(Pawn: " + pawn.Name + ")");
#endif
                JobGiver_OptimizeApparel apparelOptimizer = new JobGiver_OptimizeApparel();
                object[] param = new object[] { pawn };

                for (int i = 0; i < 10; ++i)
                {
#if TRACE && DRESSER_OUTFIT
                Log.Message(i + " start equip for loop");
#endif
                    pawn.mindState.nextApparelOptimizeTick = 0;
                    Job job = optApparelMI.Invoke(apparelOptimizer, param) as Job;
#if TRACE && DRESSER_OUTFIT
                Log.Message(i + " job is null: " + (string)((job == null) ? "yes" : "no"));
#endif
                    if (job == null)
                        break;
                    
#if TRACE && DRESSER_OUTFIT
                Log.Message(job.def.defName);
#endif
                    if (job.def == JobDefOf.Wear)
                    {
                        Apparel a = ((job.targetB != null) ? job.targetB.Thing : null) as Apparel;
                        if (a == null)
                        {
                            Log.Warning(
                                "ChangeDresser: OptimizeApparelUtil.OptimizeApparel: Problem equiping pawn. Apparel is null.");
                            break;
                        }
#if TRACE && DRESSER_OUTFIT
                    Log.Message("Wear from ground " + a.Label);
#endif
                        if (a.PawnCanWear(pawn, true))
                        {
                            pawn.apparel.Wear(a);
                        }
                    }
                    else if (job.def == Building_Dresser.WEAR_APPAREL_FROM_DRESSER_JOB_DEF)
                    {
                        Building_Dresser d = ((job.targetA != null) ? job.targetA.Thing : null) as Building_Dresser;
                        Apparel a = ((job.targetB != null) ? job.targetB.Thing : null) as Apparel;

                        if (d == null || a == null)
                        {
                            Log.Warning(
                                "ChangeDresser: OptimizeApparelUtil.OptimizeApparel: Problem equiping pawn. Dresser or Apparel is null.");
                            break;
                        }
#if TRACE && DRESSER_OUTFIT
                    Log.Message("Wear from dresser " + d.Label + " " + a.Label);
#endif
                        if (a.PawnCanWear(pawn, true))
                        {
                            d.RemoveNoDrop(a);
                            pawn.apparel.Wear(a);
                        }
                    }
#if TRACE && DRESSER_OUTFIT
                Log.Message(i + " end equip for loop");
#endif
                }
#if DRESSER_OUTFIT
            Log.Warning("End OptimizeApparelUtil.OptimizeApparel");
#endif
            }

            if (!hasDressers || pawn.apparel.WornApparel.Count == 0)
            {
                // When pawns are not on the home map they will not get dressed using the game's normal method

                // This logic works but pawns will run back to the dresser to change cloths
                foreach (ThingDef def in pawn.outfits.CurrentApparelPolicy.filter.AllowedThingDefs)
                {
#if TRACE && SWAP_APPAREL
                    Log.Warning("        Try Find Def " + def.label);
#endif
                    if (def == null)
                    {
                        continue;
                    }
                    if (pawn.apparel.CanWearWithoutDroppingAnything(def))
                    {
#if TRACE && SWAP_APPAREL
                        Log.Warning("        Can Wear. Check Dressers for apparel:");
#endif
                        foreach (Building_Dresser d in WorldComp.DressersToUse)
                        {
#if TRACE && SWAP_APPAREL
                            Log.Warning("            " + d.Label);
#endif
                            if (d.TryRemoveBestApparel(def, pawn,
                                    out Apparel apparel))
                            {
                                WorldComp.ApparelColorTracker.RemoveApparel(apparel);
#if TRACE && SWAP_APPAREL
                                Log.Warning("            Found : " + apparel.Label);
#endif
                                pawn.apparel.Wear(apparel);
                                break;
                            }
#if TRACE && SWAP_APPAREL
                            else
                                Log.Warning("            No matching apparel found");
#endif
                        }
                    }
#if TRACE && SWAP_APPAREL
                    else
                        Log.Warning("        Can't wear");
#endif
                }
            }
        }

        public static bool FindBetterApparel(
            ref float baseApparelScore, ref Apparel betterApparel, Pawn pawn, ApparelPolicy currentOutfit,
            IEnumerable<Apparel> apparelToCheck, Building dresser)
        {
            if (betterApparel == null)
                baseApparelScore = 0f;
#if BETTER_OUTFIT
            Log.Warning("Begin ApparelUtil.FindBetterApparel(Score: " + baseApparelScore + "    Apparel: " + ((betterApparel == null) ? "<null>" : betterApparel.Label));
#endif
            bool result = false;
#if TRACE && BETTER_OUTFIT
            Log.Message("    Apparel:");
#endif
            foreach (Apparel apparel in apparelToCheck)
            {
#if TRACE && BETTER_OUTFIT
                Log.Message("        " + ((apparel == null) ? "<null>" : apparel.Label));
#endif
                if (!currentOutfit.filter.Allows(apparel.def))
                {
#if TRACE && BETTER_OUTFIT
                    Log.Message("        Filters does not allow");
#endif
                    break;
                }

                if (!currentOutfit.filter.Allows(apparel) ||
                    apparel.IsForbidden(pawn))
                {
#if TRACE && BETTER_OUTFIT
                    Log.Message("        Current Outfit Does Not Allow: " + !currentOutfit.filter.Allows(apparel) + "    or     Is Forbidden: " + apparel.IsForbidden(pawn));
#endif
                    continue;
                }

#if TRACE && BETTER_OUTFIT
                Log.Message("        Keep Forced Apparel: " + Settings.KeepForcedApparel);
#endif
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                List<float> cachedValues = new List<float>(wornApparel.Count);
                if (Settings.KeepForcedApparel)
                {
                    bool skipApparelType = false;
                    foreach (Apparel a in wornApparel)
                    {
                        cachedValues.Add(JobGiver_OptimizeApparel.ApparelScoreRaw(pawn, a));
                        try
                        {
                            if (!pawn.outfits.forcedHandler.IsForced(a) &&
                                !ApparelUtility.CanWearTogether(a.def, apparel.def, pawn.RaceProps.body))
                            {
#if TRACE && BETTER_OUTFIT
                            Log.Message("        Cannot wear together");
#endif
                                skipApparelType = true;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                Log.Warning("Problem when calling CanWearTogether (" + a?.Label + ", " +
                                            apparel?.Label + ", " + pawn?.RaceProps?.body?.label + ") - " +
                                            e.GetType().Name + " " + e.Message);
                            }
                            catch
                            {
                            }

                            skipApparelType = true;
                            break;
                        }
                    }

                    if (skipApparelType)
                    {
                        break;
                    }
                }

                /*else
                {
                    foreach (Apparel a in wornApparel)
                    {
                        cachedValues.Add(JobGiver_OptimizeApparel.ApparelScoreRaw(pawn, a));
                    }
                }*/
                float gain = JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel, cachedValues);
#if TRACE && BETTER_OUTFIT
                Log.Message("    Gain: " + gain + "     Base Score: " + baseApparelScore);
#endif
                if (gain >= 0.05f && gain > baseApparelScore)
                {
#if TRACE && BETTER_OUTFIT
                    Log.Message("    Gain is better");
#endif
                    if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
                    {
#if TRACE && BETTER_OUTFIT
                        Log.Message("    Has parts to wear");
#endif
                        if (dresser == null ||
                            ReservationUtility.CanReserveAndReach(pawn, dresser, PathEndMode.OnCell,
                                pawn.NormalMaxDanger(), 1))
                        {
#if TRACE && BETTER_OUTFIT
                            Log.Message("    Can reach dresser");
#endif
                            betterApparel = apparel;
                            baseApparelScore = gain;
                            result = true;
                        }
                    }
                }
            }
#if BETTER_OUTFIT
            Log.Warning("End ApparelUtil.FindBetterApparel    result = " + result);
#endif
            return result;
        }
        /*
        public static bool TryFindBestApparel(Pawn pawn, out Apparel a, out Building_Dresser dresser)
        {
#if BETTER_OUTFIT
            Log.Warning("Begin WorldComp.TryFindBestApparel(Pawn: " + pawn.Name.ToStringShort);
#endif
            a = null;
            dresser = null;
            float baseApparelScore = 0;

            PawnOutfitTracker po;
            if (PawnOutfits.TryGetValue(pawn, out po))
            {
                ApparelUtil.FindBetterApparel(ref baseApparelScore, ref a, pawn, pawn.outfits.CurrentApparelPolicy, po.CustomApparel, null);
#if BETTER_OUTFIT
                Log.Warning("    CustomApparel Result: " + ((a == null) ? "<null>" : a.Label) + "    Score: " + baseApparelScore);
#endif
            }

            foreach (Building_Dresser d in DressersToUse)
            {
                if (d.FindBetterApparel(ref baseApparelScore, ref a, pawn, pawn.outfits.CurrentApparelPolicy))
                {
                    dresser = d;
                }
            }
#if BETTER_OUTFIT
            Log.Warning("    Dresser Result: " + ((a == null) ? "<null>" : a.Label) + "    Score: " + baseApparelScore);
#endif

#if BETTER_OUTFIT
            Log.Warning("Begin WorldComp.TryFindBestApparel -- " + ((a == null) ? "<null>" : a.Label));
#endif
            return a != null;
        }*/
    }
}