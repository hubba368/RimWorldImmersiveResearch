using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    public class Thing_FinishedExperiment : ThingWithComps
    {
        private List<ResearchTypes> ThingResearchTypes = new List<ResearchTypes>();

        private List<ResearchProjectDef> ResearchProjsForSelection = new List<ResearchProjectDef>();

        public override void PostMake()
        {
            base.PostMake();
            ThingResearchTypes = def.GetModExtension<ResearchDefModExtension>().researchTypes;
            GetResearchProjsByType();
        }

        private void GetResearchProjsByType()
        {
            if (def.HasModExtension<ResearchDefModExtension>())
            {
                if(!def.GetModExtension<ResearchDefModExtension>().researchTypes.NullOrEmpty())
                {
                    // use harmony class to select from undiscoverd list based on research types per research
                    // basically a big ass linq code block :(
                    // then choose from result list and then call add to graph func
                    // dicusting design principles and should def change but its too far gone now
                    var TempDict = LoreComputerHarmonyPatches.UndiscoveredResearchList.MainResearchDict;
                    foreach(KeyValuePair<string, ImmersiveResearchProject> p in TempDict)
                    {
                        var TempRTypes = !p.Value.ResearchTypes.NullOrEmpty() ? p.Value.ResearchTypes : null;
                        var ProjDef = p.Value.ProjectDef != null ? p.Value.ProjectDef : null;                     

                        foreach(ResearchTypes storedType in ThingResearchTypes)
                        {
                            foreach(ResearchTypes localType in TempRTypes)
                            {
                                if (localType == storedType)
                                {
                                     string tempStr = String.Format("Matching R type: {0}", localType.ToString());
                                     Log.Error(tempStr);
                                    if (ProjDef != null)
                                    {
                                        ResearchProjsForSelection.Add(ProjDef);
                                    }

                                }
                            }
                        }                       
                    }
                }
                else
                {
                    Log.Error("Finished Experiment type list is empty.");
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // generate 
        }
    }
}
