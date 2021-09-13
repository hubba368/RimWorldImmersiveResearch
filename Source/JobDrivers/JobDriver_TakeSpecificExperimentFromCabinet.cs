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
    public class JobDriver_TakeSpecificExperimentFromCabinet : JobDriver
    {
        private Building _cabinet => (Building)base.TargetThingA;

        public string RequestedExperimentDefName = null;
        public const TargetIndex CabinetIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.job.SetTarget(TargetIndex.A, _cabinet);
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(CabinetIndex);
            Building_ExperimentFilingCabinet cabinet = this.job.GetTarget(TargetIndex.A).Thing as Building_ExperimentFilingCabinet;
            RequestedExperimentDefName = LoreComputerHarmonyPatches.SpecificRequestedExperimentDefName;

            yield return Toils_Goto.GotoThing(CabinetIndex, PathEndMode.Touch);

            //this.pawn.CurJob.count = 1;

            var TakeSpecificExp = new Toil();
            TakeSpecificExp.initAction = delegate
            {
                var newExpThing = cabinet.TakeExperimentFromCabinet(RequestedExperimentDefName);
                if(newExpThing == null)
                {
                    Log.Error("exp not found in cabinet");
                    return;
                }
                var finalThing = GenSpawn.Spawn(newExpThing.def, _cabinet.Position, _cabinet.Map);

                finalThing.TryGetComp<ResearchThingComp>().pawnExperimentAuthorName = newExpThing.TryGetComp<ResearchThingComp>().pawnExperimentAuthorName;
                finalThing.TryGetComp<ResearchThingComp>().researchDefName = newExpThing.TryGetComp<ResearchThingComp>().researchDefName;

                LoreComputerHarmonyPatches.TempRequestedExp = finalThing;
            };

            yield return TakeSpecificExp;

            yield break;
        }
    }
}
