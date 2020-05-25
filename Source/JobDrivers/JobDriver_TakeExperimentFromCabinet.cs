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
    public class JobDriver_TakeExperimentFromCabinet : JobDriver
    {
        private Thing _currentFinishedExperiment = null;
        private Building _cabinet => (Building)base.TargetThingA;
        private Thing _heldThing = null;

        public Thing RequestedExperiment = null;

        public const TargetIndex CabinetIndex = TargetIndex.A;
        public const TargetIndex ExperimentIndex = TargetIndex.B;
        public const TargetIndex HeldExperimentIndex = TargetIndex.C;

        public void SetRequestedExperiment(Thing thing)
        {
            RequestedExperiment = thing;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.job.SetTarget(TargetIndex.A, _cabinet);
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(CabinetIndex);

            Building_ExperimentFilingCabinet cabinet = this.job.GetTarget(TargetIndex.A).Thing as Building_ExperimentFilingCabinet;

            var thing = LoreComputerHarmonyPatches.TempRequestedExp;

            var curExp = thing as Thing_FinishedExperiment;
            _currentFinishedExperiment = curExp;

            yield return Toils_Goto.GotoThing(CabinetIndex, PathEndMode.Touch);

            this.pawn.CurJob.count = 1;
            this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;

            // perform work toil
            var TakeExperiment = new Toil();
            TakeExperiment.initAction = delegate
            {
                var newThing = cabinet.TakeExperimentFromCabinet(curExp.TryGetComp<ResearchThingComp>().researchDefName);

                var finalThing = GenSpawn.Spawn(newThing.def, _cabinet.Position, _cabinet.Map);
                finalThing.TryGetComp<ResearchThingComp>().pawnExperimentAuthorName = newThing.TryGetComp<ResearchThingComp>().pawnExperimentAuthorName;
                finalThing.TryGetComp<ResearchThingComp>().researchDefName = newThing.TryGetComp<ResearchThingComp>().researchDefName;
            };

            yield return TakeExperiment;

            yield break;
        }
    }
}
