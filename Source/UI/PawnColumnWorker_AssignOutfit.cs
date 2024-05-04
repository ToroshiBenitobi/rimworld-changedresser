using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ChangeDresser.UI
{
  public class PawnColumnWorker_AssignOutfit : PawnColumnWorker
  {
    public const int TopAreaHeight = 65;
    public const int ManageOutfitsButtonHeight = 32;

    public override void DoHeader(Rect rect, PawnTable table)
    {
      base.DoHeader(rect, table);
      MouseoverSounds.DoRegion(rect);
      Rect rect1 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
      if (Widgets.ButtonText(rect1, (string) "ManageApparelPolicies".Translate()))
      {
        Find.WindowStack.Add((Window) new Dialog_ManageApparelPolicies((ApparelPolicy) null));
        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Outfits, KnowledgeAmount.Total);
      }
      UIHighlighter.HighlightOpportunity(rect1, "ManageOutfits");
    }

    public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
    {
      if (pawn.outfits == null)
        return;
      Rect rect1 = rect.ContractedBy(0.0f, 2f);
      int num = pawn.outfits.forcedHandler.SomethingIsForced ? 1 : 0;
      Rect left = rect1;
      Rect right = new Rect();
      if (num != 0)
        rect1.SplitVerticallyWithMargin(out left, out right, 4f);
      if (pawn.IsQuestLodger())
      {
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(left, "Unchangeable".Translate().Truncate(left.width));
        TooltipHandler.TipRegionByKey(left, "QuestRelated_Outfit");
        Text.Anchor = TextAnchor.UpperLeft;
      }
      else
        Widgets.Dropdown<Pawn, ApparelPolicy>(left, pawn, (Func<Pawn, ApparelPolicy>) (p => p.outfits.CurrentApparelPolicy), new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<ApparelPolicy>>>(this.Button_GenerateMenu), pawn.outfits.CurrentApparelPolicy.label.Truncate(left.width), dragLabel: pawn.outfits.CurrentApparelPolicy.label, paintable: true);
      if (num == 0)
        return;
      if (Widgets.ButtonText(right, (string) "ClearForcedApparel".Translate()))
        pawn.outfits.forcedHandler.Reset();
      if (!Mouse.IsOver(right))
        return;
      TooltipHandler.TipRegion(right, new TipSignal((Func<string>) (() =>
      {
        string str = (string) ("ForcedApparel".Translate() + ":\n");
        foreach (Apparel apparel in pawn.outfits.forcedHandler.ForcedApparel)
          str = str + "\n   " + apparel.LabelCap;
        return str;
      }), pawn.GetHashCode() * 612));
    }

    private IEnumerable<Widgets.DropdownMenuElement<ApparelPolicy>> Button_GenerateMenu(Pawn pawn)
    {
      foreach (ApparelPolicy allOutfit in Current.Game.outfitDatabase.AllOutfits)
      {
        ApparelPolicy outfit = allOutfit;
        yield return new Widgets.DropdownMenuElement<ApparelPolicy>()
        {
          option = new FloatMenuOption(outfit.label, (Action) (() => pawn.outfits.CurrentApparelPolicy = outfit)),
          payload = outfit
        };
      }
      yield return new Widgets.DropdownMenuElement<ApparelPolicy>()
      {
        option = new FloatMenuOption(string.Format("{0}...", (object) "AssignTabEdit".Translate()), (Action) (() => Find.WindowStack.Add((Window) new Dialog_ManageApparelPolicies(pawn.outfits.CurrentApparelPolicy))))
      };
    }

    public override int GetMinWidth(PawnTable table)
    {
      return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));
    }

    public override int GetOptimalWidth(PawnTable table)
    {
      return Mathf.Clamp(Mathf.CeilToInt(251f), this.GetMinWidth(table), this.GetMaxWidth(table));
    }

    public override int GetMinHeaderHeight(PawnTable table)
    {
      return Mathf.Max(base.GetMinHeaderHeight(table), 65);
    }

    public override int Compare(Pawn a, Pawn b)
    {
      return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
    }

    private int GetValueToCompare(Pawn pawn)
    {
      return pawn.outfits != null && pawn.outfits.CurrentApparelPolicy != null ? pawn.outfits.CurrentApparelPolicy.id : int.MinValue;
    }
  }
}