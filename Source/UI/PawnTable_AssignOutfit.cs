using RimWorld;

namespace ChangeDresser.UI
{
    public class PawnTable_AssignOutfit : PawnTable
    {
            protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
    {
      return PlayerPawnsDisplayOrderUtility.InOrder(input);
    }

    public PawnTable_PlayerPawns(
      PawnTableDef def,
      Func<IEnumerable<Pawn>> pawnsGetter,
      int uiWidth,
      int uiHeight)
      : base(def, pawnsGetter, uiWidth, uiHeight)
    {
    }
    }
}