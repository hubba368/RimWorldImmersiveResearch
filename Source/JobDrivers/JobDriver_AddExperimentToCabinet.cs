using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;

namespace ImmersiveResearch
{
    public class JobDriver_AddExperimentToCabinet : JobDriver
    {
        private Thing _currentFinishedExperiment = null;
        private Building _cabinet => (Building)base.TargetThingA;
        private Thing _heldThing = null;

        public const TargetIndex CabinetIndex = TargetIndex.A;
        public const TargetIndex ExperimentIndex = TargetIndex.B;
        public const TargetIndex HeldExperimentIndex = TargetIndex.C;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.job.SetTarget(TargetIndex.A, _cabinet);
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(CabinetIndex);

            Building_ExperimentFilingCabinet cabinet = this.job.GetTarget(TargetIndex.A).Thing as Building_ExperimentFilingCabinet;

            var thingList = LoreComputerHarmonyPatches.GetAllOfThingsOnMap("FinishedExperiment");

            if (thingList.Count == 0)
            {
                yield break;
            }

            var curExp = thingList[thingList.Count - 1] as Thing_FinishedExperiment;

            if (thingList.Count > 0)
            {// this appears to be only way to enqueue new custom jobs without using work/jobgiver
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] == curExp)
                    {
                        break;
                    }
                    var newJob = JobMaker.MakeJob(AddExperimentJobDefOf.CabinetAddExperiment, cabinet);
                    newJob.count = 1;
                    pawn.jobs.jobQueue.EnqueueFirst(newJob);
                }
            }


            _currentFinishedExperiment = curExp;
            this.job.SetTarget(ExperimentIndex, _currentFinishedExperiment);

            yield return Toils_Goto.GotoThing(ExperimentIndex, PathEndMode.Touch);
            this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;

            yield return Toils_Haul.StartCarryThing(ExperimentIndex, false, false);

            var GetHeldThing = new Toil();
            GetHeldThing.initAction = delegate
            {
                this.job.SetTarget(HeldExperimentIndex, this.pawn.carryTracker.CarriedThing);
                _heldThing = this.job.GetTarget(HeldExperimentIndex).Thing;
            };
            yield return GetHeldThing;

            yield return Toils_Goto.GotoThing(CabinetIndex, PathEndMode.Touch);
            yield return Toils_Haul.PlaceHauledThingInCell(CabinetIndex, Toils_Goto.GotoThing(CabinetIndex, PathEndMode.Touch), false);

            // perform work toil
            var AddExperiment = new Toil();
            AddExperiment.initAction = delegate
            {
                cabinet.AddExperimentToCabinet(_heldThing);
                _heldThing.Destroy();
            };

            yield return AddExperiment;
        }
    }
}
