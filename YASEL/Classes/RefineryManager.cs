using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace RefineryManager
{
    using InventoryExtensions;
    using ProgramExtensions;

    class RefineryManager
    {
        MyGridProgram gp;
        RefineryManagerSettings s;
        Dictionary<string, RefineryGroup> refineryGroups = new Dictionary<string, RefineryGroup>();
        Random rnd = new Random();

        public RefineryManager(MyGridProgram gp, RefineryManagerSettings settings = null)
        {
            this.gp = gp;
            s = settings == null ? new RefineryManagerSettings() : settings;
        }
        public void AddRefineryGroup(string oreCargoGroupName, string refineryGroupName, int baseStackSize=1000)
        {
            RefineryGroup refineryGroup = new RefineryGroup(gp, oreCargoGroupName, refineryGroupName, baseStackSize);
            if (refineryGroups.ContainsKey(oreCargoGroupName + refineryGroupName + baseStackSize))
                refineryGroups[oreCargoGroupName + refineryGroupName + baseStackSize] = refineryGroup;
            else
                refineryGroups.Add(oreCargoGroupName + refineryGroupName + baseStackSize, refineryGroup);
        }

        public void ManageRefineries()
        {
            var refineries = refineryGroups.GetEnumerator();
            while(refineries.MoveNext())
            {
                transferOreToRefineries(refineries.Current.Value);
            }
        }

        private void transferOreToRefineries(RefineryGroup refineryGroup)
        {
            if (refineryGroup.OreContainerInventories.CountItems("","Ore") == 0 || refineryGroup.RefineryInventories.GetPercentFull() > 0.95) return;


            refineryGroup.RefineryInventories.ForEach(inventory =>
            {
                if (!(inventory.Owner is IMyRefinery))
                    refineryGroup.RefineryInventories.Remove(inventory);
            });

            int numberOfOres = 0;
            refineryGroup.OreContainerInventories.ForEach(cargoInv =>
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

            refineryGroup.RefineryInventories.ForEach(refInv =>
            {
                var refItemCount = refInv.GetItems().Count;
                IMyInventoryItem firstRefItem = refItemCount>0?refInv.GetItems()[0]:null;

                if ((float)refInv.CurrentVolume / (float)refInv.MaxVolume < 0.95 && 
                    (refItemCount < numberOfOres || 
                        (numberOfOres==1 && 
                        refItemCount == 1 &&
                        firstRefItem.Amount < (VRage.MyFixedPoint)refineryGroup.BaseStackSize* (s.OreMultipliers.ContainsKey(firstRefItem.Content.SubtypeName) ? s.OreMultipliers[firstRefItem.Content.SubtypeName] : 1)))) 
                {
                    refineryGroup.OreContainerInventories.ForEach(cargoInv =>
                    {
                        var items = cargoInv.GetItems();
                        if (items.Count > 0)
                        {
                            int curIdx = 0;

                            List<invItemToMove> itemsToMove = new List<invItemToMove>();
                            items.ForEach(item =>
                            {
                                if (item.Content.TypeId.ToString().Contains("Ore"))
                                {
                                    itemsToMove.Add( new invItemToMove() { itemIdx = curIdx, itemAmount = (VRage.MyFixedPoint)refineryGroup.BaseStackSize * (s.OreMultipliers.ContainsKey(item.Content.SubtypeName) ? s.OreMultipliers[item.Content.SubtypeName] : 1) });
                                    //refInv.TransferItemFrom(cargoInv, curIdx, refInv.GetItems().Count, false,
                                      //  (VRage.MyFixedPoint)refineryGroup.BaseStackSize * (s.OreMultipliers.ContainsKey(item.Content.SubtypeName) ? s.OreMultipliers[item.Content.SubtypeName] : 1));
                                }
                                curIdx++;
                            });
                            
                            for (int itemsCount = itemsToMove.Count; itemsCount > 0; itemsCount--)
                            {
                                int nxt = rnd.Next(itemsToMove.Count);
                                refInv.TransferItemFrom(cargoInv, itemsToMove[nxt].itemIdx, refInv.GetItems().Count, false,
                                        itemsToMove[nxt].itemAmount);
                                itemsToMove.RemoveAt(nxt);
                            }
                        }
                    });
                }
            });

        }
        public class invItemToMove
        {
            public int itemIdx;
            public VRage.MyFixedPoint itemAmount;
        }
    }
    class RefineryManagerSettings
    {
        public Dictionary<string, float> OreMultipliers;
        public RefineryManagerSettings()
        {
            OreMultipliers = new Dictionary<string, float>();
            OreMultipliers.Add("Uranium", 0.005f);
            OreMultipliers.Add("Platinum", 0.033f);
            OreMultipliers.Add("Gold", 0.033f);
            OreMultipliers.Add("Magnesium", 0.1f);
            OreMultipliers.Add("Silver", 0.02f);
            OreMultipliers.Add("Iron", 1f);
            OreMultipliers.Add("Cobalt", 0.01f);
            OreMultipliers.Add("Nickel", 0.015f);
            OreMultipliers.Add("Silicon", 0.08f);
            
        }
    }
    class RefineryGroup
    {
        public List<IMyInventory> OreContainerInventories, RefineryInventories;
        public int BaseStackSize;
        public RefineryGroup(MyGridProgram gp, string oreContainerGroupName, string refineryGroupName, int baseStackSize = 1000)
        {
            BaseStackSize = baseStackSize;
            OreContainerInventories = gp.GetBlockGroup(oreContainerGroupName).GetInventories();
            RefineryInventories = gp.GetBlockGroup(refineryGroupName).GetInventories();
        }
    }
}