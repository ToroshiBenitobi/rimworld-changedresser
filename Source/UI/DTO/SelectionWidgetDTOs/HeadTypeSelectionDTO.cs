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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class HeadTypeSelectionDTO : ASelectionWidgetDTO
    {
        private List<HeadTypeDef> headTypes;
        private List<HeadTypeDef> maleHeadTypes;
        private List<HeadTypeDef> femaleHeadTypes;

        private int savedFemaleIndex = 0;
        private int savedMaleIndex = 0;

        // private List<string> maleHeadTypeStrings = new List<string>();
        // private List<string> femaleHeadTypeStrings = new List<string>();

        public readonly HeadTypeDef OriginalHeadType;
        //private List<string> crownTypes;

        public HeadTypeSelectionDTO(HeadTypeDef headType, Gender gender, Pawn_GeneTracker genes) : base()
        {
            this.OriginalHeadType = headType;
            this.maleHeadTypes = new List<HeadTypeDef>();
            this.femaleHeadTypes = new List<HeadTypeDef>();
            foreach (HeadTypeDef head in DefDatabase<HeadTypeDef>.AllDefs)
            {
                if (CanUseHeadType(head, Gender.Male, genes))
                    this.maleHeadTypes.Add(head);
                if (CanUseHeadType(head, Gender.Female, genes))
                    this.femaleHeadTypes.Add(head);
            }

            this.headTypes = (gender == Gender.Male) ? this.maleHeadTypes : this.femaleHeadTypes;
            this.Gender = gender;

            this.FindIndex(headType);
        }

        public HeadTypeSelectionDTO(HeadTypeDef headType, Gender gender, Pawn_GeneTracker genes,
            List<string> crownTypes) : this(headType, gender, genes)
        {
            this.OriginalHeadType = headType;

            // this.maleHeadTypes.AddRange(crownTypes);
            // this.femaleHeadTypes.AddRange(crownTypes);

            //this.AddHeadTypesToList("Things/Pawn/Humanlike/Heads/Male/", crownTypes);
            //this.AddHeadTypesToList("Things/Pawn/Humanlike/Heads/Female/", crownTypes);


            this.FindIndex(headType);
        }

        public void FindIndex(HeadTypeDef headType)
        {
            base.index = 0;
            for (int i = 0; i < this.headTypes.Count; ++i)
            {
                if (this.headTypes[i] == headType)
                {
                    base.index = i;
                    break;
                }
            }
        }

        public Gender Gender
        {
            set
            {
                HeadTypeDef headType = (HeadTypeDef)this.SelectedItem; 
                if (value == Gender.Female)
                {
                    this.savedMaleIndex = base.index;
                    this.headTypes = this.femaleHeadTypes;
                    base.index = savedFemaleIndex;
                }
                else
                {
                    this.savedFemaleIndex = base.index;
                    this.headTypes = this.maleHeadTypes;
                    base.index = savedMaleIndex;
                }

                this.FindIndex(headType);
                base.IndexChanged();
            }
        }

        public override int Count
        {
            get { return this.headTypes.Count; }
        }

        public override string SelectedItemLabel
        {
            get { return this.headTypes[base.index].ToString(); }
        }

        public override object SelectedItem
        {
            get { return this.headTypes[base.index]; }
        }

        private void AddHeadTypesToList(string source, List<string> list)
        {
            foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(source))
            {
                string item = current;
                if (item.IndexOf("/") == -1)
                {
                    item = source + item;
                }

                list.Add(item);
            }
        }

        public override void ResetToDefault()
        {
            this.FindIndex(this.OriginalHeadType);
            base.IndexChanged();
        }

        private bool CanUseHeadType(HeadTypeDef head, Gender gender, Pawn_GeneTracker genes)
        {
            if (ModsConfig.BiotechActive && !head.requiredGenes.NullOrEmpty<GeneDef>())
            {
                if (genes == null)
                    return false;
                foreach (GeneDef requiredGene in head.requiredGenes)
                {
                    if (genes.HasGene(requiredGene))
                        return false;
                }
            }

            return head.gender == Gender.None || head.gender == gender;
        }
    }
}