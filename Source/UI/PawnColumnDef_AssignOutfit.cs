using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ChangeDresser.UI
{
    public class PawnColumnDef_AssignOutfit : PawnColumnDef
    {
        public System.Type workerClass = typeof (PawnColumnWorker_AssignOutfit);
        public ApparelPolicy apparelPolicy;

        public bool sortable;
        public bool ignoreWhenCalculatingOptimalTableSize;
        [NoTranslate]
        public string headerIcon;
        public Vector2 headerIconSize;
        [MustTranslate]
        public string headerTip;
        public bool headerAlwaysInteractable;
        public bool paintable;
        public bool groupable;
        public TrainableDef trainable;
        public int gap;
        public WorkTypeDef workType;
        public bool moveWorkTypeLabelDown;
        public bool showIcon;
        public bool useLabelShort;
        public int widthPriority;
        public int width = -1;
        [Unsaved(false)]
        private PawnColumnWorker_AssignOutfit workerInt;
        [Unsaved(false)]
        private Texture2D headerIconTex;
        private const int IconWidth = 26;
        private static readonly Vector2 IconSize = new Vector2(26f, 26f);
        
        
        public PawnColumnWorker_AssignOutfit Worker
        {
            get
            {
                if (this.workerInt == null)
                {
                    this.workerInt = (PawnColumnWorker_AssignOutfit) Activator.CreateInstance(this.workerClass);
                    this.workerInt.def = this;
                }
                return this.workerInt;
            }
        }
    }
}