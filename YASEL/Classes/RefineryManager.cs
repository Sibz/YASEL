using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace RefineryManager
{
    using Grid;
    using Inventory;

    class RefineryManager
    {
        RefineryManagerSettings s;

        public RefineryManager()
        {
            s = new RefineryManagerSettings();
        }
        public RefineryManager(RefineryManagerSettings settings)
        {
            s = settings;
        }

        public void ManageRefineries()
        {

        }

        public void LoadRefineries(string fromGroupName, string toGroupName, int stackSize=1000)
        {
            var fromCargoBlocks = Grid.GetBlockGrp(fromGroupName);
            var toRefBlocks = Grid.GetBlockGrp(toGroupName);
            if (Inventory.CountItems(fromCargoBlocks, "Ore") == 0 || Inventory.GetPercentFull(toRefBlocks) > 0.95) return;
            
            var fromCargo = Inventory.GetInventories(fromCargoBlocks);
            var toRef = new List<IMyInventory>();
            toRefBlocks.ForEach(refBlock =>
            {
                if (refBlock is IMyRefinery)
                    toRef.Add(refBlock.GetInventory(0));
            });

            int numberOfOres = 0;
            fromCargo.ForEach(cargoInv =>
            {
                var items = cargoInv.GetItems();
                if (items.Count > 0)
                {
                    items.ForEach(item =>
                    {
                        if (item.Content.TypeId.ToString().Contains("Ore"))
                            numberOfOres++;
                    });
                }
            });

            toRef.ForEach(refInv =>
            {
                var refItemCount = refInv.GetItems().Count;
                IMyInventoryItem firstRefItem = refItemCount>0?refInv.GetItems()[0]:null;

                if ((float)refInv.CurrentVolume / (float)refInv.MaxVolume < 0.95 && 
                    (refItemCount < numberOfOres || 
                        (numberOfOres==1 && 
                        refItemCount == 1 && 
                        firstRefItem.Amount<(VRage.MyFixedPoint)stackSize*(s.OreMultipliers.ContainsKey(firstRefItem.Content.SubtypeName)?s.OreMultipliers[firstRefItem.Content.SubtypeName]:1)))) 
                {
                    fromCargo.ForEach(cargoInv =>
                    {
                        var items = cargoInv.GetItems();
                        if (items.Count > 0)
                        {
                            int curIdx = 0;
                            items.ForEach(item =>
                            {
                                if (item.Content.TypeId.ToString().Contains("Ore"))
                                {
                                    
                                    refInv.TransferItemFrom(cargoInv, curIdx, refInv.GetItems().Count, false,
                                        (VRage.MyFixedPoint)stackSize*(s.OreMultipliers.ContainsKey(item.Content.SubtypeName)?s.OreMultipliers[item.Content.SubtypeName]:1));
                                }
                                curIdx++;
                            });
                           
                        }
                    });
                }
            });


        }
    }
    class RefineryManagerSettings
    {
        public Dictionary<string, float> OreMultipliers;
        public RefineryManagerSettings()
        {
            OreMultipliers = new Dictionary<string, float>();
            OreMultipliers.Add("Uranium", 0.05f);
            OreMultipliers.Add("Platinum", 0.033f);
            OreMultipliers.Add("Gold", 0.33f);
            OreMultipliers.Add("Magnesium", 0.1f);
            OreMultipliers.Add("Silver", 0.45f);
            OreMultipliers.Add("Iron", 1f);
            OreMultipliers.Add("Cobalt", 0.1f);
            OreMultipliers.Add("Nickel", 0.33f);
            OreMultipliers.Add("Silicon", 0.5f);
            
        }
    }
}