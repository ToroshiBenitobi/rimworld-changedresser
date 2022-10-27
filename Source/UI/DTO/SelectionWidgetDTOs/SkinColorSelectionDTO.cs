using UnityEngine;

namespace ChangeDresser.UI.DTO.SelectionWidgetDTOs
{
    class SkinColorSelectionDTO : SelectionColorWidgetDTO
    {
        public ColorPresetsDTO ColorPresetsDTO { get; private set; }

        public SkinColorSelectionDTO(Color originalColor, ColorPresetsDTO presetsDto) : base(originalColor)
        {
            this.ColorPresetsDTO = presetsDto;
        }

        public new void ResetToDefault()
        {
            base.ResetToDefault();
            this.ColorPresetsDTO.Deselect();
        }
    }
}