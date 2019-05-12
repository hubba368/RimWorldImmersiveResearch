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
        private Thing ResearchDisk = null;
        private Building LoreComp => (Building)base.TargetThingA;
        public const TargetIndex LoreCompIndex = TargetIndex.A;     
        public const TargetIndex DataDiskIndex = TargetIndex.B;     

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.job.SetTarget(TargetIndex.A, LoreComp);
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(LoreCompIndex);

            Building_LoreComputer lorecomp = this.job.GetTarget(TargetIndex.A).Thing as Building_LoreComputer;
            ResearchDisk = lorecomp.GetLocationOfOwnedThing("ResearchDatadisk");
            this.job.SetTarget(DataDiskIndex, ResearchDisk);

            //haul the research datadisk to the lore computer
            yield return Toils_Goto.GotoThing(DataDiskIndex, PathEndMode.Touch);

            this.pawn.CurJob.count = 99999;
            this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;

            yield return Toils_Haul.StartCarryThing(DataDiskIndex);
            yield return Toils_Goto.GotoThing(LoreCompIndex, PathEndMode.Touch);
            yield return Toils_Haul.PlaceHauledThingInCell(LoreCompIndex, Toils_Goto.GotoThing(LoreCompIndex, PathEndMode.Touch), false);

            // perform work toil
            var loadResearchDisk = new Toil();
            loadResearchDisk.initAction = delegate
            {
                // tell the patcher to add the specific research

                LoreComputerHarmonyPatches.SelectResearchByWeighting();
                ResearchDisk.Destroy();

                // show alert on complete - research unlocked etc
                Find.LetterStack.ReceiveLetter("Research Disk Loaded", "A Research disk has been loaded, and it's research data is now usable.", LetterDefOf.PositiveEvent);
            };

            yield return loadResearchDisk;

            yield break;
        }
    }
}

