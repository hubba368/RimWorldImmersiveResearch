using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace ImmersiveResearch
{
    public class Building_ExperimentBench : Building_WorkTable
    {
        public ExperimentStack ExpStack;

        public Building_ExperimentBench()
        {
            ExpStack = new ExperimentStack(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }


        public override void TickRare()
        {
            base.TickRare();
        }
    }

}
