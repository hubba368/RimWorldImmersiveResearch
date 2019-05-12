using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace ImmersiveResearch
{
    class Building_LoreComputer : Building_WorkTable
    {
        private CompPowerTrader powerComp;
        private CompGlower glowerComp;

        public CompPowerTrader PowerComponent
        {
            get
            {
                return powerComp;
            }
        }

        public Building_LoreComputer()
        {

        }

        // Do something after object is spawned in game
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            powerComp = this.GetComp<CompPowerTrader>();
            glowerComp = GetComp<CompGlower>();

        }

        // TODO change this to locate closest one to lore computer instead of first one found in search
        public Thing GetLocationOfOwnedThing(string defOfThing)
        {
            Thing disk = null;

            List<Thing> thingList = Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
            for(int i = 0; i < thingList.Count; i++)
            {
                Thing currentThing = thingList[i];
                if(currentThing.def.defName == defOfThing)
                {
                    if (!currentThing.IsForbidden(Find.FactionManager.OfPlayer))
                    {
                        disk = currentThing;
                        return disk;
                    }
                }
            }

            return disk;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public override void TickRare()
        {
            base.TickRare();
        }

        public bool CheckIfLinkedToResearchBench()
        {
            CompProperties_Facility props = this.def.GetCompProperties<CompProperties_Facility>();
            if(props.linkableBuildings != null)
            {
                for(int i = 0; i < props.linkableBuildings.Count(); i++)
                {
                    foreach (Thing item in Map.listerThings.ThingsOfDef(props.linkableBuildings[i]))
                    {
                        CompAffectedByFacilities affectedComp = item.TryGetComp<CompAffectedByFacilities>();
                        for (int j = 0; j < affectedComp.LinkedFacilitiesListForReading.Count(); j++)
                        {
                            if (affectedComp.LinkedFacilitiesListForReading[j] == this)
                            {
                                return true;
                            }
                        }
                    }
                }                
            }
            return false;
        }

    }
}
