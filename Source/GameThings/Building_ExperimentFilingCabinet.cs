using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    public class Building_ExperimentFilingCabinet : Building
    {
        private List<string> researchDefsInCabinet; // TODO add case for when Thing list cant get required thing, use def here instead
        private Dictionary<string, Thing> experimentsInCabinet;

        private List<string> experimentDefsForScribing;
        private List<Thing> experimentThingsForScribing;

        public int ListCount => researchDefsInCabinet.Count;
        public Dictionary<string, Thing> CabinetThings => experimentsInCabinet;
        public List<string> ResearchDefsInCabinet => researchDefsInCabinet;

        public Building_ExperimentFilingCabinet()
        {
            researchDefsInCabinet = new List<string>();
            experimentsInCabinet = new Dictionary<string, Thing>();
        }

        public override string GetInspectString()
        {
            StringBuilder sBuilder = new StringBuilder();

            sBuilder.Append("Number of stored Experiments" + ": ");
            sBuilder.Append(experimentsInCabinet.Count);
            return sBuilder.ToString();
        }

        public void AddExperimentToCabinet(Thing newExp)
        {
            var experiment = (Thing_FinishedExperiment)newExp;
            var expComp = experiment.TryGetComp<ResearchThingComp>();

            if(expComp == null)
            {
                throw new NullReferenceException("finished experiment comp is null. Something must've gone wrong in crafting process. Or non crafted Thing is being used.");
            }

            if(researchDefsInCabinet.Count > 0)
            {
                for (int i = 0; i < researchDefsInCabinet.Count; i++)
                {
                    if (expComp.researchDefName == researchDefsInCabinet[i])
                    {
                       // Log.Error("research def already exists in cabinet");
                        return;
                    }
                }
            }

            researchDefsInCabinet.Add(expComp.researchDefName);
            experimentsInCabinet.Add(expComp.researchDefName, experiment);
        }

        public Thing TakeExperimentFromCabinet(string defToTake)
        {
            Thing result = null;

            if (experimentsInCabinet.ContainsKey(defToTake))
            {
                experimentsInCabinet[defToTake].def.GetModExtension<ResearchDefModExtension>().ExperimentHasBeenMade = true;
                experimentsInCabinet[defToTake].def.GetModExtension<ResearchDefModExtension>().ResearchSize = ResearchSizes.None;
                experimentsInCabinet[defToTake].def.GetModExtension<ResearchDefModExtension>().researchTypes.Clear();
                if (experimentsInCabinet[defToTake].def.GetModExtension<ResearchDefModExtension>().modResearchType != "")
                    experimentsInCabinet[defToTake].def.GetModExtension<ResearchDefModExtension>().modResearchType = "";

                var newThing = ThingMaker.MakeThing(experimentsInCabinet[defToTake].def) as Thing_FinishedExperiment;

                var comp = newThing.TryGetComp<ResearchThingComp>();
                if (comp != null)
                {
                    comp.AddPawnAuthor(experimentsInCabinet[defToTake].TryGetComp<ResearchThingComp>().pawnExperimentAuthorName);
                    comp.AddResearch(experimentsInCabinet[defToTake].TryGetComp<ResearchThingComp>().researchDefName);
                }
                result = newThing;

                experimentsInCabinet.Remove(defToTake);
                ResearchDefsInCabinet.RemoveAll(x => x == defToTake);
            }
            return result;

        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public override void TickRare()
        {
            base.TickRare();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref researchDefsInCabinet, "researchDefsInCabinet", LookMode.Value);
            // find way to suppress 'destroyed thing scribe' warning or way to use lookmode reference with collections
            // stored things are technically destroyed 
            Scribe_Collections.Look(ref experimentsInCabinet, "experimentsInCabinet", LookMode.Value, LookMode.Deep, ref experimentDefsForScribing, ref experimentThingsForScribing);

        }


    }
}
