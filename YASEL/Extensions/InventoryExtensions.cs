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
        static public List<IMyInventory> GetInventories( this MyGridProgram program)
        {
            List<IMyInventory> invs = new List<IMyInventory>();
            var blocks = new List<IMyTerminalBlock>();
            program.GridTerminalSystem.GetBlocks(blocks);
            foreach (var b in blocks)
                if (b.CubeGrid==program.Me.CubeGrid && b.HasInventory())
                {
                    for (int i = 0; i < b.GetInventoryCount(); i++)
                        invs.Add(b.GetInventory(i));
                }
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
            sources.MoveItemAmount(destinations, itemName, Math.Round((double)amount, 0), itemType, destinationMaxPercent); 
        }

        static public void MoveItemAmount(this List<IMyInventory> sources, List<IMyInventory> destinations, string itemName = "", double amount = 0, string itemType = "", float destinationMaxPercent = 0.98f, MyGridProgram gp = null)
        {
            if (amount == 0)
                amount = sources.CountItems(itemName, itemType);
            if (sources.Count == 0 || destinations.Count == 0)
                return; // No source or dest inventories
            if (sources.CountItems(itemName, itemType) == 0)
                return; // No Items to move
            amount = Math.Min(sources.CountItems(itemName, itemType), amount);
            if (destinations.GetPercentFull() > destinationMaxPercent)
                return; // No Space for items

            double amountMoved = 0;

            if (gp != null) gp.Echo("Amount to move:" + amount);

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
                                if (gp != null) gp.Echo("moving item:" + item.Content.SubtypeName);
                                if (amountMoved < amount && (item.Content.SubtypeId.ToString().Contains(itemName) &&
                                (itemType == "" || itemType.Contains(item.Content.TypeId.ToString().Replace("MyObjectBuilder_", "")))))
                                {
                                    var fpToMove = (VRage.MyFixedPoint)(item.Content.TypeId.ToString().Contains("Ingot") || item.Content.TypeId.ToString().Contains("Ore") ? (amount - amountMoved) : Math.Round((double)(amount - amountMoved), 0));
                                    toInv.TransferItemFrom(fromInv, itemIndex, null, null, fpToMove);
                                }
                                amountMoved += (itemCount - fromInv.CountItems(itemName, itemType));
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