using System.Collections.Generic;

namespace Spine.Unity
{
    public class SpineZSpacingUtils
    {
        public static void SetSlotZOrder(ExposedList<Slot> slots, List<int> zSlots)
        {
            var slotItems = slots.Items;
            var zOrder = 0;
            var zFlag = false;

            for (int i = 0, count = slots.Count; i < count; i++)
            {
                Slot slot = slotItems[i];
                if (zSlots.Contains(slot.Data.Index))
                {
                    zOrder++;
                    zFlag = true;
                }
                else if (zFlag)
                {
                    zFlag = false;
                    zOrder++;
                }

                slot.zOrder = zOrder;
            }
        }
    }
}