using RimWorld;
using UnityEngine;
using Verse;

namespace ChangeDresser.UI
{
    public class PawnColumnWorker_LabelWithCustomHead : PawnColumnWorker_Label
    {
        public override void DoHeader(Rect rect, PawnTable table)
        {
            Verse.Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(new Rect(0,0,rect.width,28), "ChangeDresser.UseForBattle".Translate());
            
            base.DoHeader(new Rect(0,28,rect.width,rect.height-28), table);
        }
    }
}