﻿using System.Collections.Generic;
using Verse;
using Verse.AI;
using ChangeDresser.UI;

namespace ChangeDresser.DresserJobDriver
{
    internal class JobDriver_StoreApparel : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    Find.WindowStack.Add(new StorageUI((Building_Dresser)base.job.targetA, this.GetActor()));
                }
            };
            yield break;
        }
    }
}
