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
    // UNUSED CLASS 
    /// <summary>
    /// This is the RimWorld equivalent to a behaviour tree
    /// Essentially makes a selected pawn attempt to interact with the lore computer object in the 
    /// world.
    /// </summary>
    class JobDriver_LoreComputer : JobDriver
    {
        private Building _loreComp => (Building)base.TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this._loreComp, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // move whatever is attempting this job to the target (lore computer)
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            // create a new database window when pawn is at destination
            var accessDatabase = new Toil();
            accessDatabase.initAction = delegate
            {
                var actor = accessDatabase.actor;
                if (!_loreComp.IsBrokenDown())
                {
                   // Log.Error("Attempting to create lore window from job driver", false);
                    Find.WindowStack.Add(new LoreComputerWindow());
                }
            };

            yield return accessDatabase;
            yield break;
        }
    }
}
