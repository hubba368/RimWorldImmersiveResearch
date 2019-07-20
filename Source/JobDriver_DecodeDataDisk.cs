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
    class JobDriver_DecodeDataDisk : JobDriver
    {
        private Thing _loreDataDisk = null;                         // ref to a data disk, either single instance or a stack
        private Building _loreComp => (Building)base.TargetThingA;
        private Thing _heldDataDisk = null;

        public const TargetIndex LoreCompIndex = TargetIndex.A;     // TargetIndex essentially holds specific info for the current Toil being
        public const TargetIndex DataDiskIndex = TargetIndex.B;     // worked on, like a ref to a Thing in game, or a list of Things, or a map cell.
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
            _loreDataDisk = lorecomp.GetLocationOfOwnedThing("LockedDatadisk");
            this.job.SetTarget(DataDiskIndex, _loreDataDisk);

            //haul the locked datadisk to the analyzer
            yield return Toils_Goto.GotoThing(DataDiskIndex, PathEndMode.Touch);

            this.pawn.CurJob.count = 1;
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

            // perform work toil (decode the datadisk)
            var decodeTheData = new Toil();
            decodeTheData.initAction = delegate
            {
                _heldDataDisk.Destroy();
                //make a new unlocked datadisk based on weighting
                // show alert when complete
                Thing temp = ThingMaker.MakeThing(LoreComputerHarmonyPatches.ChooseDataDiskTypeOnDecrypt().def);

                GenSpawn.Spawn(temp.def, _loreComp.Position, _loreComp.Map);

                Find.LetterStack.ReceiveLetter("Datadisk decoded", "A datadisk has been decoded.", LetterDefOf.PositiveEvent);
            };

            yield return decodeTheData;

            yield break;
        }
    }
}
