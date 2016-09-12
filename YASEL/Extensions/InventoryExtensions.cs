using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

namespace InventoryExtensions
{
    static class InventoryExtensions
    {
        static public float CountItems(this IMyInventory inventory, string itemName = "", string itemGroup = "")
        {
            float count = 0;
            var items = inventory.GetItems();
            items.ForEach(item =>
            {

                if (item.Content.TypeId.ToString().Contains(itemGroup) &&
                    item.Content.SubtypeName.Contains(itemName))
                    count += (float)item.Amount;
            });
            return count;
        }
        static public float CountItems(this List<IMyInventory> inventories, string itemName = "", string itemGroup = "")
        {
            float count = 0;

            inventories.ForEach(inventory =>
            {
                count += inventory.CountItems(itemName, itemGroup);
            });
            return count;
        }
        static public float GetPercentFull(this IMyInventory inventory)
        {
            return (float)inventory.CurrentVolume / (float)inventory.MaxVolume;
        }
        static public void MoveItems(this IMyInventory sources, IMyInventory destinations, string itemNames = "", string itemTypes = "", float destinationMaxPercent = 0.98f)
        {
            if (((float)destinations.CurrentVolume) / ((float)destinations.MaxVolume) > destinationMaxPercent)
                return; // can't move to full inventory, fail silently
            var items = sources.GetItems();
            if (items.Count == 0)
                return; // No Items to move
            int curIdx = 0;
            items.ForEach(item =>
            {
                if ((itemNames == "" || itemNames.Contains(item.Content.SubtypeId.ToString())) &&
                    itemTypes == "" || itemTypes.Contains(item.Content.TypeId.ToString().Replace("MyObjectBuilder_", "")))
                {
                    destinations.TransferItemFrom(sources, curIdx);
                    curIdx++;
                    if (((float)destinations.CurrentVolume) / ((float)destinations.MaxVolume) > destinationMaxPercent)
                        return;
                }
            });
        }
        static public List<IMyInventory> GetInventories(this List<IMyTerminalBlock> invBlocks, int? index = null)
        {
            List<IMyInventory> invs = new List<IMyInventory>();
            invBlocks.ForEach(inv =>
            {
                if (inv.HasInventory())
                {
                    if (index.HasValue)
                        invs.Add(inv.GetInventory(index.Value));
                    else
                        for (int i = 0; i < inv.GetInventoryCount(); i++)
                        {
                            invs.Add(inv.GetInventory(i));
                        };
                };
            });
            return invs;
        }
        static public float GetPercentFull(this List<IMyInventory> invs)
        {
            float maxVol = 0, curVol = 0;
            invs.ForEach(inv =>
            {
                maxVol += (float)inv.MaxVolume;
                curVol += (float)inv.CurrentVolume;
            });
            return curVol / maxVol;
        }
        static public void MoveItemAmount(this List<IMyInventory> sources, List<IMyInventory> destinations, string itemName, VRage.MyFixedPoint amount, string itemType = "", float destinationMaxPercent = 0.98f)
        {
            if (amount.RawValue == 0)
                amount = (VRage.MyFixedPoint)999999f;
            if (sources.Count == 0 || destinations.Count == 0)
                throw new Exception("MoveItemAmount: Unable to move, number of 'source' or 'destination' inventories = 0.");
            if (sources.CountItems(itemName, itemType) == 0)
                return; // No Items to move
            amount.RawValue = Math.Min(((VRage.MyFixedPoint)sources.CountItems(itemName, itemType)).RawValue, amount.RawValue);
            if (destinations.GetPercentFull() > destinationMaxPercent)
                return; // No Space for items

            VRage.MyFixedPoint amountMoved = 0;

            destinations.ForEach(toInv =>
            {
                if (toInv.GetPercentFull() < destinationMaxPercent && amountMoved < amount)
                {
                    sources.ForEach(fromInv =>
                    {
                        var itemCount = fromInv.CountItems(itemName, itemType);
                        if (itemCount > 0 && toInv.GetPercentFull() < destinationMaxPercent && amountMoved < amount)
                        {
                            var items = fromInv.GetItems();
                            int itemIndex = 0;
                            items.ForEach(item =>
                            {
                                if (amountMoved < amount && (item.Content.SubtypeId.ToString().Contains(itemName) &&
                                (itemType == "" || itemType.Contains(item.Content.TypeId.ToString().Replace("MyObjectBuilder_", "")))))
                                {
                                    toInv.TransferItemFrom(fromInv, itemIndex, null, null, amount - amountMoved);
                                }
                                amountMoved += (VRage.MyFixedPoint)(itemCount - fromInv.CountItems(itemName, itemType));
                                if (amountMoved >= amount)
                                    return;
                                itemIndex++;
                            });
                        }
                        else if (amountMoved >= amount)
                            return;
                    });
                }
                else if (amountMoved >= amount)
                    return;
            });
            
        }
        
    }
}