﻿/*
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

using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using ChangeDresser.UI.Enums;
using Verse;
using ChangeDresser.UI.Util;
using System.Collections.Generic;
using UnityEngine;
using System;
using RimWorld;
using System.Reflection;

namespace ChangeDresser.UI.DTO
{
    class DresserDTO
    {
        public bool HasHair { get; protected set; }

        protected long originalAgeBioTicks = long.MinValue;
        protected long originalAgeChronTicks = long.MinValue;

        public readonly Pawn Pawn;
        public CurrentEditorEnum CurrentEditorEnum { get; set; }

        public EditorTypeSelectionDTO EditorTypeSelectionDto { get; protected set; }

        public BodyTypeSelectionDTO BodyTypeSelectionDto { get; protected set; }
        public GenderSelectionDTO GenderSelectionDto { get; protected set; }
        public HairStyleSelectionDTO HairStyleSelectionDto { get; protected set; }
        public HairColorSelectionDTO HairColorSelectionDto { get; protected set; }
        public HairColorSelectionDTO GradientHairColorSelectionDto { get; protected set; }
        public ApparelColorSelectionsContainer ApparelSelectionsContainer { get; protected set; }
        public ApparelLayerSelectionsContainer ApparelLayerSelectionsContainer { get; protected set; }
        public SliderWidgetDTO SkinColorSliderDto { get; protected set; }
        public SkinColorSelectionDTO SkinColorDto { get; protected set; }
        public HeadTypeSelectionDTO HeadTypeSelectionDto { get; protected set; }

        public SelectionColorWidgetDTO AlienSkinColorPrimary { get; protected set; }
        public SelectionColorWidgetDTO AlienSkinColorSecondary { get; protected set; }
        public HairColorSelectionDTO AlienHairColorPrimary { get; protected set; }
        public HairColorSelectionDTO AlienHairColorSecondary { get; protected set; }

        public DresserDTO(Pawn pawn, CurrentEditorEnum currentEditorEnum, IEnumerable<CurrentEditorEnum> editors)
        {
            this.Pawn = pawn;

            this.CurrentEditorEnum = currentEditorEnum;
            this.EditorTypeSelectionDto =
                new EditorTypeSelectionDTO(this.CurrentEditorEnum, new List<CurrentEditorEnum>(editors));
            this.EditorTypeSelectionDto.SelectionChangeListener += delegate(object sender)
            {
                this.CurrentEditorEnum = (CurrentEditorEnum)this.EditorTypeSelectionDto.SelectedItem;
                if (this.CurrentEditorEnum == CurrentEditorEnum.ChangeDresserHair)
                {
                    Prefs.HatsOnlyOnMap = true;
                }
                else
                {
                    Prefs.HatsOnlyOnMap = false;
                }
            };

            this.HasHair = true;

            this.BodyTypeSelectionDto = null;
            this.GenderSelectionDto = null;
            this.HairStyleSelectionDto = null;
            this.HairColorSelectionDto = null;
            this.GradientHairColorSelectionDto = null;
            this.SkinColorSliderDto = null;
            this.SkinColorDto = null;
            this.HeadTypeSelectionDto = null;

            this.AlienSkinColorPrimary = null;
            this.AlienSkinColorSecondary = null;
            this.AlienHairColorPrimary = null;
            this.AlienHairColorSecondary = null;

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserApparelColor))
            {
                this.ApparelSelectionsContainer = new ApparelColorSelectionsContainer(this.Pawn.apparel.WornApparel,
                    IOUtil.LoadColorPresets(ColorPresetType.Apparel));
            }

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserApparelLayerColor))
            {
                this.ApparelLayerSelectionsContainer =
                    new ApparelLayerSelectionsContainer(this.Pawn, IOUtil.LoadColorPresets(ColorPresetType.Apparel));
            }

            this.Initialize();
        }

        protected virtual void Initialize()
        {
#if ALIEN_DEBUG
            Log.Warning("DresserDTO.initialize - start");
#endif
            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserBody))
            {

                this.originalAgeBioTicks = this.Pawn.ageTracker.AgeBiologicalTicks;
                this.originalAgeChronTicks = this.Pawn.ageTracker.AgeChronologicalTicks;
                // if (this.Pawn.ageTracker.CurLifeStage.ToString().Equals(LifeStageDefOf.HumanlikeAdult.ToString()))
                this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender,
                    this.Pawn.ageTracker.CurLifeStage);

                this.HeadTypeSelectionDto =
                    new HeadTypeSelectionDTO(this.Pawn.story.headType, this.Pawn.gender,
                        this.Pawn.genes);

                // this.SkinColorSliderDto = new SliderWidgetDTO(this.Pawn.story.melanin, 0, 1);
                ColorPresetsDTO skinColorPresets = IOUtil.LoadColorPresets(ColorPresetType.Skin);
                this.SkinColorDto = new SkinColorSelectionDTO(this.Pawn.story.SkinColorBase,
                    skinColorPresets);

                this.GenderSelectionDto = new GenderSelectionDTO(this.Pawn.gender);
                this.GenderSelectionDto.SelectionChangeListener += delegate(object sender)
                {
                    if (this.BodyTypeSelectionDto != null)
                        this.BodyTypeSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                    this.HairStyleSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                    this.HeadTypeSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                };
            }

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserHair))
            {
                this.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender);

                ColorPresetsDTO hairColorPresets = IOUtil.LoadColorPresets(ColorPresetType.Hair);
                this.HairColorSelectionDto = new HairColorSelectionDTO(this.Pawn.story.HairColor, hairColorPresets);
                if (GradientHairColorUtil.IsGradientHairAvailable)
                {
                    if (!GradientHairColorUtil.GetGradientHair(this.Pawn, out bool enabled, out Color color))
                    {
                        enabled = false;
                        color = Color.white;
                    }

                    this.GradientHairColorSelectionDto = new HairColorSelectionDTO(color, hairColorPresets, enabled);
                }
            }
        }

        public void SetUpdatePawnListeners(UpdatePawnListener updatePawn)
        {
            if (this.ApparelLayerSelectionsContainer != null)
            {
                foreach (ApparelLayerColorSelectionDTO dto in this.ApparelLayerSelectionsContainer
                             .ApparelLayerSelections)
                {
                    dto.UpdatePawnListener += updatePawn;
                }
            }

            if (this.ApparelSelectionsContainer != null)
            {
                foreach (ApparelColorSelectionDTO dto in this.ApparelSelectionsContainer.ApparelColorSelections)
                {
                    dto.UpdatePawnListener += updatePawn;
                    if (this.ApparelLayerSelectionsContainer != null)
                    {
                        dto.UpdatePawnListener += this.ApparelLayerSelectionsContainer.UpdatePawn;
                    }
                }
            }

            if (this.BodyTypeSelectionDto != null)
                this.BodyTypeSelectionDto.UpdatePawnListener += updatePawn;
            if (this.GenderSelectionDto != null)
                this.GenderSelectionDto.UpdatePawnListener += updatePawn;
            if (this.HairStyleSelectionDto != null)
                this.HairStyleSelectionDto.UpdatePawnListener += updatePawn;
            if (this.HairColorSelectionDto != null)
                this.HairColorSelectionDto.UpdatePawnListener += updatePawn;
            if (this.GradientHairColorSelectionDto != null)
                this.GradientHairColorSelectionDto.UpdatePawnListener += updatePawn;
            if (this.SkinColorSliderDto != null)
                this.SkinColorSliderDto.UpdatePawnListener += updatePawn;
            if (this.SkinColorDto != null)
                this.SkinColorDto.UpdatePawnListener += updatePawn;
            if (this.HeadTypeSelectionDto != null)
                this.HeadTypeSelectionDto.UpdatePawnListener += updatePawn;
            if (this.AlienSkinColorPrimary != null)
                this.AlienSkinColorPrimary.UpdatePawnListener += updatePawn;
            if (this.AlienSkinColorSecondary != null)
                this.AlienSkinColorSecondary.UpdatePawnListener += updatePawn;
            if (this.AlienHairColorPrimary != null)
                this.AlienHairColorPrimary.UpdatePawnListener += updatePawn;
            if (this.AlienHairColorSecondary != null)
                this.AlienHairColorSecondary.UpdatePawnListener += updatePawn;
        }

        public void ResetToDefault()
        {
#if TRACE
            Log.Warning(System.Environment.NewLine + "DresserDTO.Begin ResetToDefault");
#endif
            // Gender must happen first
            if (this.GenderSelectionDto != null)
                this.GenderSelectionDto.ResetToDefault();
            if (this.BodyTypeSelectionDto != null)
                this.BodyTypeSelectionDto.ResetToDefault();
            if (this.HairStyleSelectionDto != null)
                this.HairStyleSelectionDto.ResetToDefault();
            if (this.HairColorSelectionDto != null)
                this.HairColorSelectionDto.ResetToDefault();
            if (this.GradientHairColorSelectionDto != null)
                this.GradientHairColorSelectionDto.ResetToDefault();
            if (this.ApparelSelectionsContainer != null)
                this.ApparelSelectionsContainer.ResetToDefault();
            if (this.ApparelLayerSelectionsContainer != null)
                this.ApparelLayerSelectionsContainer.ResetToDefault();
            if (this.SkinColorSliderDto != null)
                this.SkinColorSliderDto.ResetToDefault();
            if (this.SkinColorDto != null)
                this.SkinColorDto.ResetToDefault();
            if (this.HeadTypeSelectionDto != null)
                this.HeadTypeSelectionDto.ResetToDefault();

            if (this.originalAgeBioTicks != long.MinValue)
                this.Pawn.ageTracker.AgeBiologicalTicks = this.originalAgeBioTicks;
            this.BodyTypeSelectionDto.LifeStage = this.Pawn.ageTracker.CurLifeStage;
            if (this.originalAgeChronTicks != long.MinValue)
                this.Pawn.ageTracker.AgeChronologicalTicks = this.originalAgeChronTicks;

            if (this.AlienSkinColorPrimary != null)
                this.AlienSkinColorPrimary.ResetToDefault();
            if (this.AlienSkinColorSecondary != null)
                this.AlienSkinColorSecondary.ResetToDefault();
            this.AlienHairColorPrimary?.ResetToDefault();
            this.AlienHairColorSecondary?.ResetToDefault();
#if TRACE
            Log.Warning("End DresserDTO.ResetToDefault" + System.Environment.NewLine);
#endif
        }

        // internal virtual void SetCrownType(object value)
        // {
        //     if (value.ToString().IndexOf("Narrow") >= 0 ||
        //         value.ToString().IndexOf("narrow") >= 0)
        //     {
        //         this.Pawn.story.crownType = CrownType.Narrow;
        //     }
        //     else
        //     {
        //         this.Pawn.story.crownType = CrownType.Average;
        //     }
        //     typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.Pawn.story, value);
        // } //todo:crawn type
    }
}