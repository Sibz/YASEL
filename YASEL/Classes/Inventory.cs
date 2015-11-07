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
        public static float MaxInventoryPercent = 0.98f; // Max percent of an inventory, after this stop trying to move items to that inventory.

        static public float CountItems(List<IMyTerminalBlock> invBlocks, string itemType = "", string itemSubtypeName = "")
        {
            var invs = new List<IMyInventory>();
            invBlocks.ForEach(invBlock =>
            {
                if (invBlock.HasInventory())
                {
                    for (int i = 0; i < invBlock.GetInventoryCount(); i++)
                    {
                        invs.Add(invBlock.GetInventory(i));
                    }
                }
            });
            return CountItems(invs);
        }
        static public float CountItems(List<IMyInventory> invs, string itemType = "", string itemSubtypeName = "")
        {
            float count = 0;
            invs.ForEach(inv =>
            {
                count += CountItems(inv);
            });
            return count;
        }
        static public float CountItems(IMyInventory inv, string itemType = "", string itemSubtypeName = "")
        {
            float count = 0;
            var items = inv.GetItems();
            items.ForEach(item =>
            {
                if (item.Content.TypeId.ToString().Contains(itemType) &&
                    item.Content.SubtypeName.Contains(itemSubtypeName))
                    count += (float)item.Amount;
            });
            return count;
        }

        static public float GetPercentFull(List<IMyTerminalBlock> invBlocks)
        {
            var listInvs = GetInventories(invBlocks);
            return GetPercentFull(listInvs);

        }

        static public float GetPercentFull(List<IMyInventory> invs)
        {
            float maxVol = 0, curVol = 0;
            invs.ForEach(inv =>
            {
                maxVol += (float)inv.MaxVolume;
                curVol += (float)inv.CurrentVolume;
            });
            return curVol/maxVol;
        }
        static public float GetPercentFull(IMyInventory inv)
        {
            return (float)inv.CurrentVolume / (float)inv.MaxVolume;
        }

        static public List<IMyInventory> GetInventories(List<IMyTerminalBlock> invBlocks)
        {
            List<IMyInventory> invs = new List<IMyInventory>();
            invBlocks.ForEach(inv =>
            {
                if (inv.HasInventory())
                {
                    for (int i = 0; i < inv.GetInventoryCount(); i++)
                    {
                        invs.Add(inv.GetInventory(i));
                    };
                };
            });
            return invs;
        }

        static public void MoveItems(IMyInventory invFrom, IMyInventory invTo, string itemNames="", string itemTypes="")
        {
            if (((float)invTo.CurrentVolume) / ((float)invTo.MaxVolume) > MaxInventoryPercent)
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
                    if (((float)invTo.CurrentVolume) / ((float)invTo.MaxVolume) > MaxInventoryPercent)
                        return;
                }
            });
        }

        static public void MoveItemAmount(List<IMyTerminalBlock> fromBlocks, List<IMyTerminalBlock> toBlocks, string itemName, VRage.MyFixedPoint amount, string? itemType=null)
        {
            var fromInvs = GetInventories(fromBlocks);
            var toInvs = GetInventories(toBlocks);
            MoveItemAmount(fromInvs, toInvs, itemName, amount, itemType);
        }

        static public void MoveItemAmount(List<IMyInventory> fromInvs, List<IMyInventory> toInvs, string itemName, VRage.MyFixedPoint amount, string? itemType = null)
        {
            if (fromInvs.Count == 0 || toInvs.Count == 0)
                throw new Exception("MoveItemAmount: Unable to move, number of 'from' or 'to' inventories = 0.");
            if (CountItems(fromInvs,itemType.HasValue?itemType.Value:"",itemName) == 0)
                return; // No Items to move
            if (GetPercentFull(toInvs) > MaxInventoryPercent)
                return; // No Space for items

            VRage.MyFixedPoint amountMoved = 0;

            toInvs.ForEach(toInv =>
            {
                if (GetPercentFull(toInv) < MaxInventoryPercent && amountMoved < amount)
                {
                    fromInvs.ForEach(fromInv =>
                    {
                        var itemCount = CountItems(fromInv,itemType.HasValue?itemType.Value:itemName);
                        if (itemCount > 0 && GetPercentFull(toInv) < MaxInventoryPercent && amountMoved < amount)
                        {
                            var items = fromInv.GetItems();
                            int itemIndex = 0;
                            items.ForEach(item =>
                            {
                                if (amountMoved<amount && (item.Content.SubtypeId.ToString().Contains(itemName) &&
                                (!itemType.HasValue || itemType.Value.Contains(item.Content.TypeId.ToString().Replace("MyObjectBuilder_","")))))
                                {
                                    toInv.TransferItemFrom(fromInv, itemIndex, null, null, amount - amountMoved);
                                }
                                amountMoved += (VRage.MyFixedPoint)(itemCount - CountItems(fromInv, itemType.HasValue ? itemType.Value : itemName));
                                itemIndex++;
                            });
                        }
                    });
                }
            });

        }

    }
}