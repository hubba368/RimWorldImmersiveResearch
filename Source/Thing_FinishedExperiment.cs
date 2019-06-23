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
            if (!ThingResearchTypes.NullOrEmpty())
            {
                GetResearchProjsByType();
                SelectResearch();
            }
            
        }

        private void SelectResearch()
        {
            LoreComputerHarmonyPatches.SelectResearchByWeightingAndType(ResearchProjsForSelection, this);
        }

        private void GetResearchProjsByType()
        {
            List<ResearchProjectDef> tempProjList = new List<ResearchProjectDef>();
            if (def.HasModExtension<ResearchDefModExtension>())
            {
                if(!def.GetModExtension<ResearchDefModExtension>().researchTypes.NullOrEmpty())
                {
                    // use harmony class to select from undiscoverd list based on research types per research
                    // should def refactor this
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
                                    //string tempStr = String.Format("Matching R type: {0}", localType.ToString());
                                    //Log.Error(tempStr);
                                    if (ProjDef != null)
                                    {
                                        tempProjList.Add(ProjDef);
                                    }

                                }
                            }
                        }                       
                    }
                    // now get list of projs based on size
                    var prunedList = tempProjList.Where(item => item.GetModExtension<ResearchDefModExtension>().ResearchSize == this.def.GetModExtension<ResearchDefModExtension>().ResearchSize);

                    ResearchProjsForSelection.AddRange(prunedList);
                }
                else
                {
                    Log.Error("Finished Experiment type list is empty.");
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy();
        }
    }
}
