using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RimWorld;
using Verse;


namespace ImmersiveResearch
{
    public class ResearchDict : IExposable
    {
        private static Dictionary<string, ImmersiveResearchProject> UndiscoveredResearchList = new Dictionary<string, ImmersiveResearchProject>();

        private List<string> ResearchDictKeys = new List<string>();
        private List<bool> ResearchDictBools = new List<bool>();
        private List<float> ResearchDictWeightings = new List<float>();

        //TODO: figure out better way to scribe list of lists if possible
        private Dictionary<int, ResearchTypes> researchTypesLocal = new Dictionary<int, ResearchTypes>();

        public Dictionary<string, ImmersiveResearchProject> MainResearchDict
        {
            get
            {
                return UndiscoveredResearchList;
            }
            set
            {
                UndiscoveredResearchList = value;
            }
        }

        // we only save vars that can be saved by the scribing system
        private void PrepareDictForScribing()
        {
            ResearchDictKeys.Clear();
            ResearchDictBools.Clear();
            ResearchDictWeightings.Clear();

            for (int i = 0; i < UndiscoveredResearchList.Count(); i++)
            {
                ResearchDictKeys.Add(UndiscoveredResearchList.ElementAt(i).Key);
                ResearchDictBools.Add(UndiscoveredResearchList.ElementAt(i).Value.IsDiscovered);
                ResearchDictWeightings.Add(UndiscoveredResearchList.ElementAt(i).Value.Weighting);
            }
        }

        private void LoadListsToDict()
        {
            UndiscoveredResearchList.Clear();
            for(int i = 0; i < ResearchDictKeys.Count(); i++)
            {
                var projToAdd  = LoreComputerHarmonyPatches.TempResearchList.Where(item => item.defName == ResearchDictKeys[i]);

                if(!(projToAdd.Count() < 0))
                {
                    ResearchProjectDef proj = projToAdd.ElementAt(0);
                    UndiscoveredResearchList.Add(ResearchDictKeys[i], new ImmersiveResearchProject(proj, ResearchDictBools[i], ResearchDictWeightings[i]));
                }
                else
                {
                    Log.Error("A research project def was null/not found.");
                }
            }
        }

        public void ExposeData()
        {
            PrepareDictForScribing();
            Scribe_Collections.Look(ref ResearchDictKeys, "MainResearchDictKeys", LookMode.Value);
            Scribe_Collections.Look(ref ResearchDictBools, "MainResearchDictBools", LookMode.Value);
            Scribe_Collections.Look(ref ResearchDictWeightings, "MainResearchDictWeightings", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                //Log.Error("loading function running");
                LoadListsToDict();
            }
        }
    }
}
