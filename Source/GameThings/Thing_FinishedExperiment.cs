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
        private List<ResearchTypes> _thingResearchTypes = new List<ResearchTypes>();
        private ResearchSizes _thingResearchSize = 0;
        private List<ResearchProjectDef> _researchProjsForSelection = new List<ResearchProjectDef>();

        public List<ResearchTypes> ThingResearchTypes { get => _thingResearchTypes; set => _thingResearchTypes = value; }
        public ResearchSizes ThingResearchSize { get => _thingResearchSize; set => _thingResearchSize = value; }

        public override void PostMake()
        {
            //TODO: move this from post make so a NEW research proj isnt discovered every time an new instance of this appears in the game world
            _thingResearchTypes = def.GetModExtension<ResearchDefModExtension>().researchTypes;   
            _thingResearchSize = def.GetModExtension<ResearchDefModExtension>().ResearchSize;

            //Log.Error(_thingResearchSize.ToString());
            //Log.Error(_thingResearchTypes[0].ToString());
            if (def.GetModExtension<ResearchDefModExtension>().ExperimentHasBeenMade)
            {
                // Log.Error("experiment has been made already");
                
            }
            else
            {
                if (!_thingResearchTypes.NullOrEmpty())
                {
                    GetResearchProjsByType();
                }
                else
                {
                    Log.Error("Thing_FinishedExperiment failed to get research types from recipe.");
                }
            }
            base.PostMake();
        }

        public override string GetInspectString()
        {
            StringBuilder sBuilder = new StringBuilder();
            var comp = GetComp<ResearchThingComp>();

            if(comp.pawnExperimentAuthorName != null)
            {
                sBuilder.Append("Most Recent Experiment Author" + ": ");
                sBuilder.Append(comp.pawnExperimentAuthorName + "\n");
                sBuilder.Append("Research Discovered" + ": ");
                sBuilder.Append(comp.researchDefName);
            }


            return sBuilder.ToString();
        }

        private void SelectResearch()
        {
            var temp = LoreComputerHarmonyPatches.SelectResearchByWeightingAndType(_researchProjsForSelection);
            def.GetModExtension<ResearchDefModExtension>().ResearchDefAttachedToExperiment = temp;
        }

        private void GetResearchProjsByType()
        {
            if (def.HasModExtension<ResearchDefModExtension>())
            {
                if (!def.GetModExtension<ResearchDefModExtension>().researchTypes.NullOrEmpty())
                {
                    List<ImmersiveResearchProject> tempProjList = new List<ImmersiveResearchProject>();
                    var TempDict = LoreComputerHarmonyPatches.UndiscoveredResearchList.MainResearchDict;

                    if (_thingResearchTypes[0] == ResearchTypes.Mod)
                    {
                        tempProjList = TempDict.Values.ToList();
                        tempProjList.RemoveAll(item => item.ResearchTypes[0] != ResearchTypes.Mod);

                        var finalList = new List<ResearchProjectDef>();

                        for (int i = 0; i < tempProjList.Count; i++)
                        {
                            finalList.Add(tempProjList[i].ProjectDef);
                        }
                        _researchProjsForSelection.AddRange(finalList);
                        SelectResearch();
                    }
                    else
                    {
                        var t = TempDict.Values.ToList().Where(item => item.ResearchSize == _thingResearchSize);
                        var finalSearchSpace = t.Where(item => item.ResearchTypes.Any(x => x == _thingResearchTypes[0]));

                        foreach(ImmersiveResearchProject p in finalSearchSpace)
                        {

                            var ProjDef = p.ProjectDef != null ? p.ProjectDef : null;

                            if (ProjDef != null)
                            {
                                if (!TempDict.ContainsKey(ProjDef.defName))
                                {
                                    //Log.Error("cant find key: " + ProjDef.defName);
                                    continue;
                                }
                                if (TempDict[ProjDef.defName].IsDiscovered == true)
                                {
                                    //Log.Error("is discovered: " + ProjDef.defName);
                                    continue;
                                }
                                else
                                {
                                    //Log.Error("adding proj: " + ProjDef.defName);
                                    tempProjList.Add(TempDict[ProjDef.defName]);
                                }
                            }
                        }

                        if (tempProjList.Count > 0)
                        {
                            var finalList = new List<ResearchProjectDef>();

                            for (int i = 0; i < tempProjList.Count(); i++)
                            {
                                finalList.Add(tempProjList[i].ProjectDef);
                            }

                            _researchProjsForSelection.AddRange(finalList);
                            SelectResearch();
                        }
                        else
                        {
                            //Log.Error("no researches are undiscovered");
                            Find.LetterStack.ReceiveLetter("Experiment Completed", "An experiment has been completed, and unfortunately nothing has been discovered.", LetterDefOf.NeutralEvent);
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
            base.Destroy();
        }

        //TODO: expose researchthingcomp
        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var comp = GetComp<ResearchThingComp>();

                Scribe_Collections.Look(ref _thingResearchTypes, "ResearchTypes", LookMode.Value);
                Scribe_Values.Look(ref _thingResearchSize, "ResearchSize");
                Scribe_Values.Look(ref comp.pawnExperimentAuthorName, "ExperimentAuthor");
                //Scribe_Defs.Look(ref comp.researchDef, "ExperimentDef");
                Scribe_Values.Look(ref comp.researchDefName, "ExperimentDefName");
            }

            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                var comp = GetComp<ResearchThingComp>();

                Scribe_Collections.Look(ref _thingResearchTypes, "ResearchTypes", LookMode.Value);
                Scribe_Values.Look(ref _thingResearchSize, "ResearchSize");
                Scribe_Values.Look(ref comp.pawnExperimentAuthorName, "ExperimentAuthor");
               // Scribe_Defs.Look(ref comp.researchDef, "ExperimentDef");
                Scribe_Values.Look(ref comp.researchDefName, "ExperimentDefName");
            }
        }
    }
}
