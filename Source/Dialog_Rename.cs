using System;
using RimWorld;
using Verse;

namespace ChangeDresser
{
    // class Dialog_Rename : Dialog_Rename<Building_Dresser>
    // {
    //     private Building_Dresser Dresser;
    //
    //     public Dialog_Rename(Building_Dresser dresser) : base(dresser)
    //     {
    //         this.Dresser = dresser;
    //         base.curName = dresser.RenamableLabel;
    //     }
    //
    //     protected override AcceptanceReport NameIsValid(string name)
    //     {
    //         AcceptanceReport acceptanceReport = base.NameIsValid(name);
    //         if (!acceptanceReport.Accepted)
    //             return acceptanceReport;
    //         // return name != this.renaming.label && this.map.animalPenManager.GetPenNamed(name) != null ? (AcceptanceReport) "NameIsInUse".Translate() : (AcceptanceReport) true;
    //         return Current.Game.CurrentMap.zoneManager.AllZones.Any<Zone>((Predicate<Zone>) (z => z.label == name)) || Current.Game.CurrentMap.storageGroups.HasGroupWithName(name) ? (AcceptanceReport) "NameIsInUse".Translate() : (AcceptanceReport) true;
    //     }
    //     
    //     protected override void OnRenamed(string name)
    //     {
    //         this.Dresser.RenamableLabel = name;
    //     }
    // }
}
