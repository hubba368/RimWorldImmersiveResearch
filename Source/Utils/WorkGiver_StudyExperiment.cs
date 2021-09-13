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
        private List<Thing> _cabinets = LoreComputerHarmonyPatches.GetAllOfThingsOnMap("ExperimentFilingCabinet");

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
            _cabinets = LoreComputerHarmonyPatches.GetAllOfThingsOnMap("ExperimentFilingCabinet");

            for (int i = 0; i < billGiver.BillStack.Count; i++)
            {
                var table = (Building_StudyTable)billGiver;
                var bill = billGiver.BillStack[i];
                var exp = table.ExpStack.Experiments[table.ExpStack.IndexOfBillToExp(bill)];

                if (bill.PawnAllowedToStartAnew(pawn))
                {                   
                    List<ThingCount> temp = new List<ThingCount>();
                    var uniqueExpThing = exp.uniqueThingIng;
                    string rDefName = exp.uniqueThingIng.def.GetModExtension<ResearchDefModExtension>().ResearchDefAttachedToExperiment;

                    if (uniqueExpThing.stackCount == 0)
                    {
                        if (_cabinets == null)
                        {
                            // Messages.Message("No Built Experiment Filing Cabinet(s) in Colony.", MessageTypeDefOf.RejectInput, historical: false);
                            Log.Error("No experiment cabinets in colony. Job will attempt to retrieve experiment from any on map");
                        }
                        else
                        {// TODO: loop through cabinets on map if more than one
                            for(int j = 0; j < _cabinets.Count; j++)
                            {
                                var cabinet = _cabinets[j] as Building_ExperimentFilingCabinet;
                                if (cabinet.ListCount != 0 && cabinet.ResearchDefsInCabinet.Contains(rDefName))
                                {
                                    LoreComputerHarmonyPatches.SpecificRequestedExperimentDefName = rDefName;
                                    var job = JobMaker.MakeJob(TakeSpecificExperimentJobDefOf.CabinetTakeSpecificExperiment, cabinet);
                                    return job;
                                }
                                else
                                {
                                    Log.Error("either cabinet has nothing or does not have correct exp to take");
                                }
                            }

                        }// BIG TODO!!!!!!! REMOVE STATIC VAR AND USE SOME KINDA FUNC TO PASS THIS!!!
                        uniqueExpThing = LoreComputerHarmonyPatches.TempRequestedExp;
                    }
                    
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
