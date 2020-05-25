using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace ImmersiveResearch
{
    class JobDriver_LoadResearchDisk : JobDriver
    {
        private Thing _researchDisk = null;
        private Building _loreComp => (Building)base.TargetThingA;
        private Thing _heldDataDisk = null;

        public const TargetIndex LoreCompIndex = TargetIndex.A;     
        public const TargetIndex DataDiskIndex = TargetIndex.B;
        public const TargetIndex HeldDiskIndex = TargetIndex.C;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.job.SetTarget(TargetIndex.A, _loreComp);
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(LoreCompIndex);

            Building_LoreComputer lorecomp = this.job.GetTarget(TargetIndex.A).Thing as Building_LoreComputer;
            _researchDisk = lorecomp.GetLocationOfOwnedThing("ResearchDatadisk");
            this.job.SetTarget(DataDiskIndex, _researchDisk);

            yield return Toils_Goto.GotoThing(DataDiskIndex, PathEndMode.Touch);

            this.pawn.CurJob.count = 1; // this controls the num of times the pawn will do the toils, e.g count of 50 will make pawn do the entire job 50 times
            this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;

            yield return Toils_Haul.StartCarryThing(DataDiskIndex, false, false);

            var GetHeldDisk = new Toil();
            GetHeldDisk.initAction = delegate
            {
                this.job.SetTarget(HeldDiskIndex, this.pawn.carryTracker.CarriedThing);
                _heldDataDisk = this.job.GetTarget(HeldDiskIndex).Thing;
            };
            yield return GetHeldDisk;

            yield return Toils_Goto.GotoThing(LoreCompIndex, PathEndMode.Touch);
            yield return Toils_Haul.PlaceHauledThingInCell(LoreCompIndex, Toils_Goto.GotoThing(LoreCompIndex, PathEndMode.Touch), false);

            // perform work toil
            var loadResearchDisk = new Toil();
            loadResearchDisk.initAction = delegate
            {
                _heldDataDisk.Destroy();
                // get a random research from entire list
                string result = LoreComputerHarmonyPatches.SelectResearchByUniformCumulativeProb(LoreComputerHarmonyPatches.UndiscoveredResearchList.MainResearchDict.Values.ToList());
                LoreComputerHarmonyPatches.AddNewResearch(result);

                Find.LetterStack.ReceiveLetter("Research Disk Loaded", "A Research disk has been loaded, and it's research data is now usable.", LetterDefOf.PositiveEvent);
            };

            yield return loadResearchDisk;

            yield break;
        }
    }
}

