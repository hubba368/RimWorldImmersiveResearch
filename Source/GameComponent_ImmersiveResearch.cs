using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    public class GameComponent_ImmersiveResearch : GameComponent
    {
        private ResearchDict _researchDict;
        private Dictionary<string, List<string>> _colonyResearcherExperimentDict = new Dictionary<string, List<string>>();
        private List<string> _colonyExperimentAuthorsForSaving = new List<string>();
        private List<string> _colonyExperimentDefNamesForSaving = new List<string>();

        public ResearchDict MainResearchDict
        {
            get
            {
                return _researchDict;
            }
        }
        public Dictionary<string, List<string>> ColonyResearcherExperimentDict { get => _colonyResearcherExperimentDict;}



        public GameComponent_ImmersiveResearch(Game game) : base()
        {
            _researchDict = new ResearchDict();
        }

        public bool CheckResearcherHasPublishedExperiments(string pawn)
        {
            if (_colonyResearcherExperimentDict.ContainsKey(pawn))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckResearcherAuthoredSpecificExperiment(string pawn ,string def)
        {
            var list = _colonyResearcherExperimentDict[pawn].Where(x => x == def);
            for(int i = 0; i < list.Count(); i++)
            {
                if(list.ElementAt(i) == def)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddColonyExperimentToPawn(string researcher, string researchDef)
        {
            var tempName = researcher;

            if (CheckResearcherHasPublishedExperiments(researcher))
            {
                if(!CheckResearcherAuthoredSpecificExperiment(researcher, researchDef))
                {
                    _colonyResearcherExperimentDict[tempName].Add(researchDef);
                }
            }
            else
            {
                var temp = new List<string>();
                temp.Add(researchDef);
                _colonyResearcherExperimentDict.Add(tempName, temp);
            }
        }

        public void ColonyResearcherDeath(string pawn)
        {
            var pawnName = pawn;

            /*foreach(var temp in _colonyResearcherExperimentDict)
            {
                Log.Error(temp.Key);
                foreach(var temp2 in temp.Value)
                {
                    Log.Error(temp2);
                }
            }*/

            var currentProjectsInColony = _colonyResearcherExperimentDict[pawnName];
            var tempList = LoreComputerHarmonyPatches.FullConcreteResearchList;
            var researchDefList = new List<Tuple<ResearchProjectDef,int>>();

            foreach (var def in currentProjectsInColony)
            {
                var list = tempList.Where(x => x.defName == def);
                var tuple = new Tuple<ResearchProjectDef, int>(list.ElementAt(0), CheckNumOfAuthorsOnExperiment(list.ElementAt(0).defName));
                researchDefList.Add(tuple);
            }

            foreach (var tuple in researchDefList)
            {
                var proj = tuple.Item1;
                var numOfAuthors = tuple.Item2;

                if(numOfAuthors > 1)
                {// TODO: move to seperate func
                    if (_researchDict.MainResearchDict[proj.defName].IsDiscovered)
                    {
                        if (proj.ProgressReal == 0.0f)
                        { 
                            //_researchDict.MainResearchDict[proj.defName].IsDiscovered = false;
                        }

                        if (proj.IsFinished || proj.ProgressReal != 0.0f)
                        {
                            var curProj = Find.ResearchManager.currentProj;
                            if (curProj == null)
                            {
                                //Log.Error("proj is null");
                                Find.ResearchManager.currentProj = proj;
                            }

                            float amount = GenerateProgressLossPerAuthor(numOfAuthors, Find.ResearchManager.currentProj.CostApparent);
                            float finalAmount = (amount * Find.ResearchManager.currentProj.CostApparent) / 0.00825f;
                            Find.ResearchManager.ResearchPerformed(-finalAmount, null);
                            Find.ResearchManager.currentProj = curProj;
                        }
                    }
                }
                else
                {
                    if (_researchDict.MainResearchDict[proj.defName].IsDiscovered)
                    {
                        if (proj.ProgressReal == 0.0f)
                        {
                            Find.LetterStack.ReceiveLetter("Colony Researcher Death - Sole Author", proj.defName + " had one author. Unfortunately, the project has been lost.", LetterDefOf.NegativeEvent);
                            _researchDict.MainResearchDict[proj.defName].IsDiscovered = false;
                        }

                        if (proj.IsFinished || proj.ProgressReal != 0.0f)
                        {
                            var curProj = Find.ResearchManager.currentProj;
                            if (curProj == null)
                            {
                                Find.ResearchManager.currentProj = proj;
                            }
                            Find.ResearchManager.ResearchPerformed(Find.ResearchManager.currentProj.ProgressReal / -0.00825f, null);
                            Find.ResearchManager.currentProj = curProj;
                        }
                    }
                }
            }

            RemoveResearcherFromDict(pawnName);
        }

        private int CheckNumOfAuthorsOnExperiment(string rDef)
        {
            int result = 0;

            for(int i = 0; i < _colonyResearcherExperimentDict.Count; ++i)
            {
                string pawn = _colonyResearcherExperimentDict.Keys.ToList()[i];
                for (int j = 0; j < _colonyResearcherExperimentDict[pawn].Count; ++j)
                {
                    if(_colonyResearcherExperimentDict[pawn][j] == rDef)
                    {
                        result++;
                    }
                }
            }

            return result;
        }

        private float GenerateProgressLossPerAuthor(int authorCount, float progressLoss)
        {
            float result = 0;

            // more authors per project, less progress lost
            var totalProgressLost = 1f;

            for(int i = 1; i < authorCount; i++)
            {
                totalProgressLost -= progressLoss;
            }

            result += totalProgressLost;

            return result;
        }

        private void RemoveResearcherFromDict(string key)
        {
            _colonyResearcherExperimentDict.Remove(key);
        }

        public string GetPawnUniqueName(Pawn pawn)
        {
            var name = pawn.Name as NameTriple;
            var tempName = "";

            tempName = name != null ? pawn.Name.ToString() : name.First + name.Last;

            return tempName;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            _researchDict.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _colonyExperimentAuthorsForSaving = _colonyResearcherExperimentDict.Keys.ToList();

                string currentExpAuthor = "";
                foreach (var t in _colonyResearcherExperimentDict)
                {
                    currentExpAuthor = t.Key;
                    if (t.Value.Count > 0)
                    {
                        for (int i = 0; i < t.Value.Count; ++i)
                        {
                            string temp = currentExpAuthor + "_" + t.Value[i];
                            _colonyExperimentDefNamesForSaving.Add(temp);
                        }
                    }
                }

                Scribe_Collections.Look(ref _colonyExperimentAuthorsForSaving, "ColonyCompletedExperimentsAuthors", LookMode.Value);
                Scribe_Collections.Look(ref _colonyExperimentDefNamesForSaving, "ColonyCompletedExperimentsDefNames", LookMode.Value);
            }

            //Scribe_Collections.Look(ref _colonyResearcherExperimentDict, "ColonyCompletedExperimentsDict", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Collections.Look(ref _colonyExperimentAuthorsForSaving, "ColonyCompletedExperimentsAuthors", LookMode.Value);
                Scribe_Collections.Look(ref _colonyExperimentDefNamesForSaving, "ColonyCompletedExperimentsDefNames", LookMode.Value);

                if(_colonyExperimentAuthorsForSaving != null && _colonyExperimentDefNamesForSaving != null)
                {
                    foreach (var t in _colonyExperimentAuthorsForSaving)
                    {
                        // TODO: change this so it doesnt use chars that could be used in a pawn name
                        //       Or to something else entirely
                        List<string> tempList = new List<string>();
                        _colonyResearcherExperimentDict.Add(t, tempList);
                        foreach (var j in _colonyExperimentDefNamesForSaving)
                        {
                            string newDef = j.Substring(j.LastIndexOf("_" )+ 1);
                          //  Log.Error(newDef);
                            tempList.Add(newDef);
                        }
                    }
                }


            }         
        }
    }
}
