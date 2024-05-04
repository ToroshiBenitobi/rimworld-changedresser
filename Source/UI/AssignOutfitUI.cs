using ChangeDresser.UI.Util;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChangeDresser.UI
{
    class AssignOutfitUI : Window
    {
        private readonly Building_Dresser Dresser;
        private Vector2 scrollPosition = new Vector2(0, 0);
        private float PreviousY = 0;
        private readonly Color BorderColor = new Color(1f, 1f, 1f, 0.2f);

        private bool dirty;
        public void SetDirty() => this.dirty = true;

        private List<Pawn> cachedPawns = new List<Pawn>();
        private PawnColumnDef sortByColumn;
        private bool sortDescending;
        private List<PawnColumnDef_AssignOutfit> columns = new List<PawnColumnDef_AssignOutfit>();

        private List<float> cachedColumnWidths = new List<float>();
        private List<float> cachedRowHeights = new List<float>();
        private List<LookTargets> cachedLookTargets = new List<LookTargets>();
        private Vector2 cachedSize;
        private float cachedHeaderHeight;
        private float cachedHeightNoScrollbar;

        private const int NAME_WIDTH = 100;
        private const int CHECKBOX_WIDTH = 100;
        private const int HEIGHT = 35;
        private const int X_BUFFER = 10;
        private const int Y_BUFFER = 5;
        private Rect windowRect;


        public AssignOutfitUI(Building_Dresser dresser)
        {
            this.Dresser = dresser;

            this.closeOnClickedOutside = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.closeOnClickedOutside = false;

            foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Where<Pawn>(
                         (Func<Pawn, bool>)(pawn => !pawn.DevelopmentalStage.Baby())))
            {
                if (p.Faction == Faction.OfPlayer && p.def.race.Humanlike && p.apparel?.LockedApparel?.Count == 0)
                {
                    if (!WorldComp.PawnOutfits.ContainsKey(p))
                    {
                        PawnOutfitTracker po = new PawnOutfitTracker();
                        po.Pawn = p;
                        ApparelPolicy currentOutfit = p.outfits.CurrentApparelPolicy;
                        if (currentOutfit != null)
                        {
                            po.AddOutfit(new DefinedOutfit(currentOutfit, WorldComp.GetOutfitType(currentOutfit)));
                        }

                        WorldComp.PawnOutfits.Add(p, po);
                    }
                }
            }

            SetDirty();
        }

        public override Vector2 InitialSize
        {
            get { return new Vector2(650f, 600f); }
        }

        private void RecacheIfDirty()
        {
            if (!this.dirty)
                return;
            this.dirty = false;
            this.RecachePawns();
            this.RecacheRowHeights();
            this.cachedHeaderHeight = this.CalculateHeaderHeight();
            this.cachedHeightNoScrollbar = this.CalculateTotalRequiredHeight();
            this.RecacheSize();
            this.RecacheColumnWidths();
            this.RecacheLookTargets();
        }


        public void TableGUI(Vector2 position)
        {
            float num1 = this.cachedSize.x - 16f;
            List<PawnColumnDef_AssignOutfit> columns = this.Columns;
            int num2 = 0;
            for (int index = 0; index < columns.Count; ++index)
            {
                int width = index != columns.Count - 1
                    ? (int)this.cachedColumnWidths[index]
                    : (int)((double)num1 - (double)num2);
                Rect rect = new Rect((float)((int)position.x + num2), (float)(int)position.y, (float)width,
                    (float)(int)this.cachedHeaderHeight);
                columns[index].Worker.DoHeader(rect, this);
                num2 += width;
            }

            Rect outRect = new Rect((float)(int)position.x, (float)((int)position.y + (int)this.cachedHeaderHeight),
                (float)(int)this.cachedSize.x, (float)((int)this.cachedSize.y - (int)this.cachedHeaderHeight));
            Rect viewRect = new Rect(0.0f, 0.0f, outRect.width - 16f,
                (float)((int)this.cachedHeightNoScrollbar - (int)this.cachedHeaderHeight));
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
            int x = 0;
            for (int index1 = 0; index1 < columns.Count; ++index1)
            {
                int y = 0;
                PawnColumnDef_AssignOutfit pawnColumnDef = columns[index1];
                int num3 = index1 != columns.Count - 1
                    ? (int)this.cachedColumnWidths[index1]
                    : (int)((double)num1 - (double)x);
                for (int index2 = 0; index2 < this.cachedPawns.Count; ++index2)
                {
                    GUI.color = this.BorderColor;
                    Widgets.DrawLineHorizontal((float)x, (float)y, (float)num3);
                    GUI.color = Color.white;
                    Rect rect = new Rect((float)x, (float)y, (float)num3,
                        (float)(int)this.cachedRowHeights[index2]);
                    Pawn cachedPawn = this.cachedPawns[index2];
                    bool flag = false;


                    if (((double)y - (double)this.scrollPosition.y + (double)(int)this.cachedRowHeights[index2] <
                         0.0
                            ? 1
                            : ((double)y - (double)this.scrollPosition.y > (double)outRect.height ? 1 : 0)) == 0)
                    {
                        columns[index1].Worker.DoCell(rect, cachedPawn, this);
                    }

                    GUI.color = Color.white;
                    y += (int)rect.height;
                }

                x += num3;
            }

            int y1 = 0;
            for (int index = 0; index < this.cachedPawns.Count; ++index)
            {
                Rect rect = new Rect(0.0f, (float)y1, viewRect.width, (float)(int)this.cachedRowHeights[index]);
                if (Find.Selector.IsSelected((object)this.cachedPawns[index]))
                    Widgets.DrawHighlight(rect, 0.6f);
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                    this.cachedLookTargets[index].Highlight(colonistBar: this.cachedPawns[index].IsColonist);
                }

                if (this.cachedPawns[index].Downed)
                {
                    GUI.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
                    Widgets.DrawLineHorizontal(0.0f, rect.center.y, viewRect.width);
                    GUI.color = Color.white;
                }

                y1 += (int)this.cachedRowHeights[index];
            }

            Widgets.EndScrollView();
        }

        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                this.windowRect = inRect;
                this.RecacheIfDirty();


                //bool useInApparelLookup = this.Dresser.UseInApparelLookup;
                //Widgets.CheckboxLabeled(new Rect(0, 0, 300, HEIGHT), "ChangeDresser.UseAsApparelSource".Translate(), ref useInApparelLookup);
                //this.Dresser.UseInApparelLookup = useInApparelLookup;

                if (Widgets.ButtonText(new Rect( /*450*/0, 0, 150, HEIGHT), "ChangeDresser.ManageOutfits".Translate()))
                {
                    Find.WindowStack.Add(
                        new Dialog_ManageApparelPolicies(null /*Current.Game.outfitDatabase.DefaultOutfit*/));
                }

                this.TableGUI(new Vector2(inRect.x, inRect.y + HEIGHT));
            }
            catch (Exception e)
            {
                Log.Error(e.GetType() + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public void DoWindowContentsOld(Rect inRect)
        {
            try
            {
                this.windowRect = inRect;
                this.RecacheIfDirty();
                const int NAME_WIDTH = 100;
                const int CHECKBOX_WIDTH = 100;
                const int HEIGHT = 35;
                const int X_BUFFER = 10;
                const int Y_BUFFER = 5;

                //bool useInApparelLookup = this.Dresser.UseInApparelLookup;
                //Widgets.CheckboxLabeled(new Rect(0, 0, 300, HEIGHT), "ChangeDresser.UseAsApparelSource".Translate(), ref useInApparelLookup);
                //this.Dresser.UseInApparelLookup = useInApparelLookup;

                if (Widgets.ButtonText(new Rect( /*450*/0, 0, 150, HEIGHT), "ChangeDresser.ManageOutfits".Translate()))
                {
                    Find.WindowStack.Add(
                        new Dialog_ManageApparelPolicies(null /*Current.Game.outfitDatabase.DefaultOutfit*/));
                }

                List<ApparelPolicy> allOutfits = Current.Game.outfitDatabase.AllOutfits;
                int y = 50 + HEIGHT + Y_BUFFER;

                GUI.BeginScrollView(
                    new Rect(0, y, inRect.width - 32, HEIGHT * 2 + Y_BUFFER * 3),
                    this.scrollPosition,
                    new Rect(0, y,
                        NAME_WIDTH + X_BUFFER + ((CHECKBOX_WIDTH + X_BUFFER) * allOutfits.Count),
                        HEIGHT * 2 + Y_BUFFER * 3),
                    GUIStyle.none, GUIStyle.none);

                // Header - Lists the Outfit names
                int x = NAME_WIDTH + X_BUFFER;
                //y = 0;
                foreach (ApparelPolicy o in allOutfits)
                {
                    //Widgets.Label(new Rect(x, y, NAME_WIDTH, HEIGHT), "Pawns".Translate());
                    //x += CHECKBOX_WIDTH + X_BUFFER;
                    Widgets.Label(new Rect(x, y, CHECKBOX_WIDTH, HEIGHT), o.label);
                    x += CHECKBOX_WIDTH + X_BUFFER;
                }

                y += HEIGHT + Y_BUFFER;

                // Use For Battle row
                x = 0;
                Widgets.Label(new Rect(x, y, NAME_WIDTH, HEIGHT), "ChangeDresser.UseForBattle".Translate());
                x += NAME_WIDTH + X_BUFFER;
                foreach (ApparelPolicy o in allOutfits)
                {
                    bool use = WorldComp.OutfitsForBattle.Contains(o);
                    bool useNoChange = use;
                    Widgets.Checkbox(x + 10, y, ref use);
                    x += CHECKBOX_WIDTH + X_BUFFER;

                    if (use != useNoChange)
                    {
                        if (use)
                        {
                            WorldComp.OutfitsForBattle.Add(o);
                        }
                        else
                        {
                            bool removed = WorldComp.OutfitsForBattle.Remove(o);
                        }

                        foreach (PawnOutfitTracker po in WorldComp.PlayFunctionPawnOutfits.Values)
                        {
                            po.UpdateOutfitType(o, (use) ? OutfitType.Battle : OutfitType.Civilian);
                        }
                    }
                }

                y += HEIGHT + Y_BUFFER * 2;
                Widgets.DrawLineHorizontal(NAME_WIDTH + X_BUFFER, y - 4,
                    NAME_WIDTH + X_BUFFER + ((CHECKBOX_WIDTH + X_BUFFER) * allOutfits.Count));
                GUI.EndScrollView();

                // Pawn Names
                GUI.BeginScrollView(
                    new Rect(0, y, NAME_WIDTH, inRect.height - y - 90),
                    new Vector2(0, scrollPosition.y),
                    new Rect(0, 0, NAME_WIDTH, this.PreviousY),
                    GUIStyle.none, GUIStyle.none);
                x = 0;
                int py = 0;
                foreach (PawnOutfitTracker po in WorldComp.PlayFunctionPawnOutfits.Values)
                {
                    Widgets.Label(new Rect(x, py, NAME_WIDTH, HEIGHT), po.Pawn.Name.ToStringShort);
                    py += HEIGHT + Y_BUFFER;
                }

                this.PreviousY = py;
                Widgets.DrawLineVertical(NAME_WIDTH + 2, y,
                    (HEIGHT + Y_BUFFER) * WorldComp.PlayFunctionPawnOutfits.Values.Count);
                GUI.EndScrollView();

                int mainScrollXMin = NAME_WIDTH + X_BUFFER + 4;
                this.scrollPosition = GUI.BeginScrollView(
                    new Rect(mainScrollXMin, y, inRect.width - mainScrollXMin, inRect.height - y - 90),
                    this.scrollPosition,
                    new Rect(0, 0,
                        NAME_WIDTH + X_BUFFER + ((CHECKBOX_WIDTH + X_BUFFER) * allOutfits.Count) - mainScrollXMin,
                        this.PreviousY));

                // Table of pawns and assigned outfits
                py = 0;
                foreach (PawnOutfitTracker po in WorldComp.PlayFunctionPawnOutfits.Values)
                {
                    x = 0;
                    foreach (ApparelPolicy o in allOutfits)
                    {
                        bool assign = po.Contains(o);
                        bool assignNoChange = assign;
                        Widgets.Checkbox(x + 10, py, ref assign);
                        x += CHECKBOX_WIDTH + X_BUFFER;

                        if (assign != assignNoChange)
                        {
                            this.HandleOutfitAssign(assign, o, po);
                        }

                        /*
                        bool assign = WorldComp.OutfitsForBattle.Contains(o);
                        Widgets.Checkbox(x + 10, y, ref assign);
                        if (Widgets.ButtonInvisible(new Rect(x + 5, y + 5, CHECKBOX_WIDTH - 5, HEIGHT - 5)))
                        {
                            this.HandleOutfitAssign(!WorldComp.OutfitsForBattle.Contains(o), o, po);
                        }
                        */
                    }

                    py += HEIGHT + Y_BUFFER;
                }

                GUI.EndScrollView();
            }
            catch (Exception e)
            {
                Log.Error(e.GetType() + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private void HandleOutfitAssign(bool assign, ApparelPolicy outfit, PawnOutfitTracker po)
        {
            Pawn pawn = po.Pawn;
            if (assign)
            {
                po.DefinedOutfits.Add(new DefinedOutfit(outfit, WorldComp.GetOutfitType(outfit)));
            }
            else
            {
                po.Remove(outfit);
                if (pawn.outfits.CurrentApparelPolicy.Equals(outfit))
                {
                    bool newOutfitFound;
                    if (pawn.Drafted)
                    {
                        newOutfitFound = !po.ChangeToBattleOutfit();
                    }
                    else
                    {
                        newOutfitFound = !po.ChangeToCivilianOutfit();
                    }

                    if (!newOutfitFound)
                    {
                        Messages.Message(
                            pawn.Name.ToStringShort + " will no longer wear " + outfit.label +
                            ". Could not find another Outfit for them to wear. Please fix this manually.",
                            MessageTypeDefOf.CautionInput);
                    }
                    else
                    {
                        IDresserOutfit o = po.CurrentOutfit;
                        if (o != null)
                        {
                            Messages.Message(
                                pawn.Name.ToStringShort + " will no longer wear " + outfit.label +
                                " and will instead be assigned to wear " + o.Label, MessageTypeDefOf.CautionInput);
                        }
                        else
                        {
                            Messages.Message(
                                pawn.Name.ToStringShort + " will no longer wear " + outfit.label +
                                " but could not be assigned anything else to wear.", MessageTypeDefOf.CautionInput);
                        }
                    }
                }
            }
        }

        public List<PawnColumnDef_AssignOutfit> Columns
        {
            get
            {
                this.columns.Clear();
                foreach (ApparelPolicy outfit in Current.Game.outfitDatabase.AllOutfits)
                {
                    PawnColumnDef_AssignOutfit def = new PawnColumnDef_AssignOutfit();
                    def.apparelPolicy = outfit;
                    this.columns.Add(def);
                }
                return this.columns;
            }
        }

        private IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
        {
            return PlayerPawnsDisplayOrderUtility.InOrder(input);
        }

        private void RecachePawns()
        {
            this.cachedPawns.Clear();
            this.cachedPawns.AddRange(
                PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Where<Pawn>(
                    (Func<Pawn, bool>)(pawn => !pawn.DevelopmentalStage.Baby())));
            this.cachedPawns = this.LabelSortFunction((IEnumerable<Pawn>)this.cachedPawns).ToList<Pawn>();
            if (this.sortByColumn != null)
            {
                if (this.sortDescending)
                    this.cachedPawns.SortStable<Pawn>(new Func<Pawn, Pawn, int>(this.sortByColumn.Worker.Compare));
                else
                    this.cachedPawns.SortStable<Pawn>((Func<Pawn, Pawn, int>)((a, b) =>
                        this.sortByColumn.Worker.Compare(b, a)));
            }
            // this.cachedPawns = this.PrimarySortFunction((IEnumerable<Pawn>) this.cachedPawns).ToList<Pawn>();
        }

        private void RecacheSize()
        {
            this.cachedSize = new Vector2(this.windowRect.width, HEIGHT * 2 + Y_BUFFER * 3);
        }

        private void RecacheRowHeights()
        {
            this.cachedRowHeights.Clear();
            for (int index = 0; index < this.cachedPawns.Count; ++index)
                this.cachedRowHeights.Add(this.CalculateRowHeight(this.cachedPawns[index]));
        }

        private void RecacheColumnWidths()
        {
            float totalAvailableSpaceForColumns = this.cachedSize.x - 16f;
            float minWidthsSum = 0.0f;
            this.cachedColumnWidths.Clear();
            List<PawnColumnDef_AssignOutfit> columns = this.Columns;
            for (int index = 0; index < columns.Count; ++index)
            {
                float minWidth = this.GetMinWidth(columns[index]);
                this.cachedColumnWidths.Add(minWidth);
                minWidthsSum += minWidth;
            }
        }

        private float GetMinWidth(PawnColumnDef_AssignOutfit column)
        {
            return Mathf.Max((float)column.Worker.GetMinWidth(this), 0.0f);
        }

        private void RecacheLookTargets()
        {
            this.cachedLookTargets.Clear();
            this.cachedLookTargets.AddRange(
                this.cachedPawns.Select<Pawn, LookTargets>((Func<Pawn, LookTargets>)(p => new LookTargets((Thing)p))));
        }

        private float CalculateHeaderHeight()
        {
            float a = 0.0f;
            List<PawnColumnDef_AssignOutfit> columns = this.Columns;
            for (int index = 0; index < columns.Count; ++index)
                a = Mathf.Max(a, (float)columns[index].Worker.GetMinHeaderHeight(this));
            return a;
        }

        private float CalculateRowHeight(Pawn pawn)
        {
            float a = 0.0f;
            List<PawnColumnDef_AssignOutfit> columns = this.Columns;
            for (int index = 0; index < columns.Count; ++index)
                a = Mathf.Max(a, (float)columns[index].Worker.GetMinCellHeight(pawn));
            return a;
        }

        private float CalculateTotalRequiredHeight()
        {
            float headerHeight = this.CalculateHeaderHeight();
            for (int index = 0; index < this.cachedPawns.Count; ++index)
                headerHeight += this.CalculateRowHeight(this.cachedPawns[index]);
            return headerHeight;
        }
    }
    
}