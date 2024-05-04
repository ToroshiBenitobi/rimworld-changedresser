using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ChangeDresser.UI
{
    public class PawnTable_AssignOutfit : PawnTable
    {
            protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
    {
      return PlayerPawnsDisplayOrderUtility.InOrder(input);
    }

    public PawnTable_AssignOutfit(
      PawnTableDef def,
      Func<IEnumerable<Pawn>> pawnsGetter,
      int uiWidth,
      int uiHeight)
      : base(def, pawnsGetter, uiWidth, uiHeight)
    {
    }
    }
}