using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Inventory
{
    class Inventory
    {
        static public float CountItems(List<IMyTerminalBlock> invBlocks, string itemType = "", string itemSubtypeName = "")
        {
            float count = 0;
            invBlocks.ForEach(invBlock =>
            {
                if (invBlock is IMyInventoryOwner)
                {
                    for (int i = 0; i < (invBlock as IMyInventoryOwner).InventoryCount; i++)
                    {
                        var items = (invBlock as IMyInventoryOwner).GetInventory(i).GetItems();
                        items.ForEach(item =>
                        {
                            if (item.Content.TypeId.ToString().Contains(itemType) &&
                                item.Content.SubtypeName.Contains(itemSubtypeName))
                                count += (float)item.Amount;
                        });
                    }
                }
            });
            return count;
        }

        static public List<IMyInventory> GetInventories(List<IMyTerminalBlock> invBlocks)
        {
            List<IMyInventory> invs = new List<IMyInventory>();
            invBlocks.ForEach(inv =>
            {
                if (inv is IMyInventoryOwner)
                {
                    for (int i = 0; i < (inv as IMyInventoryOwner).InventoryCount; i++)
                    {
                        invs.Add((inv as IMyInventoryOwner).GetInventory(i));
                    };
                };
            });
            return invs;
        }

        static public void MoveItems(IMyInventory invFrom, IMyInventory invTo, string itemNames="", string itemTypes="", float maxPercent = 98)
        {
            if (((float)invTo.CurrentVolume) / ((float)invTo.MaxVolume) > (maxPercent/100))
                return; // can't move to full inventory, fail silently
            var items = invFrom.GetItems();
            if (items.Count == 0)
                return; // No Items to move
            int curIdx = 0;
            items.ForEach(item =>
            {
                if ((itemNames == "" || itemNames.Contains(item.Content.SubtypeId.ToString())) &&
                    itemTypes == "" || itemTypes.Contains(item.Content.TypeId.ToString().Replace("MyObjectBuilder_","")))
                {
                    invTo.TransferItemFrom(invFrom, curIdx);
                    curIdx++;
                    if (((float)invTo.CurrentVolume) / ((float)invTo.MaxVolume) > (maxPercent / 100))
                        return;
                }
            });
        }
    }
}