/*
 * MIT License
 * 
 * Copyright (c) [2017] [Travis Offtermatt]
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class BodyTypeSelectionDTO : ASelectionWidgetDTO
    {
        public readonly BodyTypeDef OriginalBodyType;

        private List<BodyTypeDef> bodyTypes;
        private List<BodyTypeDef> maleBodyTypes;
        private List<BodyTypeDef> femaleBodyTypes;
        private List<BodyTypeDef> babyBodyTypes;
        private List<BodyTypeDef> ChildBodyTypes;

        public BodyTypeSelectionDTO(BodyTypeDef bodyType, Gender gender, LifeStageDef lifeStage) : base()
        {
            this.OriginalBodyType = bodyType;

            this.maleBodyTypes = new List<BodyTypeDef>()
            {
                BodyTypeDefOf.Male,
                BodyTypeDefOf.Thin,
                BodyTypeDefOf.Hulk,
                BodyTypeDefOf.Fat,
                // BodyTypeDefOf.Baby,
                // BodyTypeDefOf.Child
            };
            this.femaleBodyTypes = new List<BodyTypeDef>()
            {
                BodyTypeDefOf.Female,
                BodyTypeDefOf.Thin,
                BodyTypeDefOf.Hulk,
                BodyTypeDefOf.Fat,
                // BodyTypeDefOf.Baby,
                // BodyTypeDefOf.Child
            };
            this.ChildBodyTypes = new List<BodyTypeDef>()
            {
                BodyTypeDefOf.Child
            };
            this.babyBodyTypes = new List<BodyTypeDef>()
            {
                BodyTypeDefOf.Baby
            };
            // Log.Message(lifeStage.ToString());
            this.bodyTypes = (gender == Gender.Male) ? this.maleBodyTypes : this.femaleBodyTypes;
            // if (lifeStage.ToString().Equals(LifeStageDefOf.HumanlikeBaby.ToString()))
            // {
            //     this.bodyTypes = this.babyBodyTypes;
            // }
            // else if (lifeStage.ToString().Equals(LifeStageDefOf.HumanlikeChild.ToString()))
            // {
            //     this.bodyTypes = this.ChildBodyTypes;
            // }
            // else
            // {
            //     
            // }

            this.FindIndex(bodyType);
        }

        public BodyTypeSelectionDTO(
            BodyTypeDef bodyType, Gender gender, List<BodyTypeDef> possibleBodyTypes) : base()
        {
            this.OriginalBodyType = bodyType;

            this.maleBodyTypes = new List<BodyTypeDef>(possibleBodyTypes.Count - 1);
            this.femaleBodyTypes = new List<BodyTypeDef>(possibleBodyTypes.Count - 1);
            foreach (BodyTypeDef bt in possibleBodyTypes)
            {
                if (bt != BodyTypeDefOf.Female)
                    this.maleBodyTypes.Add(bt);
                if (bt != BodyTypeDefOf.Male)
                    this.femaleBodyTypes.Add(bt);
            }

            this.bodyTypes = (gender == Gender.Male) ? this.maleBodyTypes : this.femaleBodyTypes;
            this.FindIndex(bodyType);
        }

        private void FindIndex(BodyTypeDef bodyType)
        {
            base.index = 0;
            for (int i = 0; i < this.bodyTypes.Count; ++i)
            {
                if (this.bodyTypes[i] == bodyType)
                {
                    base.index = i;
                    break;
                }
            }
        }

        public LifeStageDef LifeStage
        {
            set
            {
                // BodyTypeDef bodyType = (BodyTypeDef)this.SelectedItem;
                //
                // if (value.ToString().Equals(LifeStageDefOf.HumanlikeBaby.ToString()))
                // {
                //     this.bodyTypes = this.babyBodyTypes;
                // }
                // else if (value.ToString().Equals(LifeStageDefOf.HumanlikeChild.ToString()))
                // {
                //     this.bodyTypes = this.ChildBodyTypes;
                // }
                // else
                // {
                //     this.bodyTypes = (this.Gender == Gender.Male) ? this.maleBodyTypes : this.femaleBodyTypes;
                // }
                //
                // this.FindIndex(bodyType);
                // base.IndexChanged();
            }
        }

        public Gender Gender
        {
            set
            {
                BodyTypeDef bodyType = (BodyTypeDef)this.SelectedItem;
                if (value == Gender.Female)
                {
                    this.bodyTypes = this.femaleBodyTypes;
                    if (bodyType == BodyTypeDefOf.Male)
                    {
                        bodyType = BodyTypeDefOf.Female;
                    }
                }
                else // Male
                {
                    this.bodyTypes = this.maleBodyTypes;
                    if (bodyType == BodyTypeDefOf.Female)
                    {
                        bodyType = BodyTypeDefOf.Male;
                    }
                }

                this.FindIndex(bodyType);
                base.IndexChanged();
            }
            get { return this.Gender; }
        }

        public override int Count
        {
            get { return this.bodyTypes.Count; }
        }

        public override string SelectedItemLabel
        {
            get { return this.bodyTypes[base.index].ToString(); }
        }

        public override object SelectedItem
        {
            get { return this.bodyTypes[base.index]; }
        }

        public override void ResetToDefault()
        {
            this.FindIndex(this.OriginalBodyType);
            base.IndexChanged();
        }
    }
}