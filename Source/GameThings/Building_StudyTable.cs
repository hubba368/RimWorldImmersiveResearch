using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    public class Building_StudyTable : Building_WorkTable
    {
        public ExperimentStack ExpStack;

        public Building_StudyTable()
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref ExpStack, "studyStack", this);
        }
    }
}
