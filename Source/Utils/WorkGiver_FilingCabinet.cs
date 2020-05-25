using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace ImmersiveResearch
{
    public class WorkGiver_FilingCabinet : WorkGiver_Scanner
    {
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return base.ShouldSkip(pawn, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // get localtarget cabinet and search map for all finished exps;
            if(t.def.defName != "ExperimentFilingCabinet")
            {
                return null;
            }

            Job newJob = JobMaker.MakeJob(TakeExperimentJobDefOf.CabinetTakeExperiment);
            return null;
        }
    }
}
