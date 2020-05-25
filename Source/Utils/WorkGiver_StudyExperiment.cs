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
    public class WorkGiver_StudyExperiment : WorkGiver_DoBill
    {
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return base.ShouldSkip(pawn, forced);
        }


        // check if work table is usable in any way
        // for each bill check if current pawn is assigned to the bill
        // if it is get the unique Thing from the experiment and pass it to the work bill
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            IBillGiver billGiver = thing as IBillGiver;
            if (billGiver == null || !ThingIsUsableBillGiver(thing) || !billGiver.BillStack.AnyShouldDoNow || !billGiver.UsableForBillsAfterFueling() || !pawn.CanReserve(thing, 1, -1, null, forced) || thing.IsBurning() || thing.IsForbidden(pawn))
            {
                return null;
            }

            for(int i = 0; i < billGiver.BillStack.Count; i++)
            {
                var table = (Building_StudyTable)billGiver;
                var bill = billGiver.BillStack[i];
                var exp = table.ExpStack.Experiments[table.ExpStack.IndexOfBillToExp(bill)];

                if (bill.PawnAllowedToStartAnew(pawn))
                {                   
                    List<ThingCount> temp = new List<ThingCount>();
                    var uniqueExpThing = exp.uniqueThingIng;

                    // Get unique exp thing per bill
                    if (billGiver.BillStack.Bills.Count > 0)
                    {// TODO need workaround for pausing a study in progress
                        // trystartnewbill resets progress
                        // probs just multiply/divide recipe cost based on intellectual level
                        temp.Add(new ThingCount(uniqueExpThing, 1));
                        return TryStartNewDoBillJob(pawn, bill, billGiver, temp, out Job haulOffJob);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }


            return null;
        }
    }
}
