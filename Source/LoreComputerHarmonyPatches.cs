using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using UnityEngine;

namespace ImmersiveResearch
{
    public enum ResearchTypes
    {
        None,
        Biological,
        Mechanical,
        Textiles,
        Cultural,
        Construction,
        Metallurgy,
        Carpentry,
        Weaponry,
        Masonry,
        Electrical,
        Medical,
        Spacecraft,
        Advanced,
        Spacer,
        Ultra,
        Mod
    }

    public enum ResearchSizes
    {
        Small,
        Medium,
        Large,
        Essential,
        Unknown,
        None
    }
    // potential todo: make this inherit ILoadReferenceable to make it easier to scribe?
    public class ImmersiveResearchProject
    {
        ResearchProjectDef _projectDef;
        bool _isDiscovered;
        float _weighting;
        List<ResearchTypes> _researchTypes;
        ResearchSizes _researchSize;

        public ResearchProjectDef ProjectDef
        {
            get
            {
                return _projectDef;
            }
            set
            {
                _projectDef = value;
            }
        }

        public bool IsDiscovered
        {
            get
            {
                return _isDiscovered;
            }
            set
            {
                _isDiscovered = value;
            }
        }

        public float Weighting
        {
            get
            {
                return _weighting;
            }
            set
            {
                _weighting = value;
            }
        }

        public List<ResearchTypes> ResearchTypes
        {
            get
            {
                return _researchTypes;
            }
            set
            {
                _researchTypes = value;
            }
        }

        public ResearchSizes ResearchSize { get; set; }

        public ImmersiveResearchProject(ResearchProjectDef projDef, bool discovered, float weight, List<ResearchTypes> rTypes, ResearchSizes rSize)
        {
            _projectDef = projDef;
            _isDiscovered = discovered;
            _weighting = weight;
            _researchTypes = rTypes;
            _researchSize = rSize;
        }

        public ImmersiveResearchProject(bool discovered, float weight, List<ResearchTypes> rTypes, ResearchSizes rSize)
        {
            _isDiscovered = discovered;
            _weighting = weight;
            _researchTypes = rTypes;
            _researchSize = rSize;
        }

        public ImmersiveResearchProject(ResearchProjectDef projDef, bool discovered, float weight)
        {
            _projectDef = projDef;
            _isDiscovered = discovered;
            _weighting = weight;
        }

        public ImmersiveResearchProject(ResearchProjectDef projDef)
        {
            _projectDef = projDef;
        }
    }





    /// <summary>
    /// Class that uses Harmony to inject custom code into RimWorld code.
    /// </summary>
    [StaticConstructorOnStartup]
    static class LoreComputerHarmonyPatches
    {

        private static float _neolithicProbability = 40.0f;
        private static float _medievalProbability = 20.0f;
        private static float _industrialProbability = 10.0f;
        private static float _spacerAndAboveProbability = 5.0f;
        private static float _rareDatadiskProbability = 50.0f;
        private static float _superRareDatadiskProbability = 5.0f;

        public static List<ResearchProjectDef> FullConcreteResearchList = new List<ResearchProjectDef>(DefDatabase<ResearchProjectDef>.AllDefsListForReading); // a concrete list of all possible research options

        public static ResearchDict UndiscoveredResearchList;
        
        static LoreComputerHarmonyPatches()
        {          
           
            var harmony = new Harmony("rimworld.mods.immersiveresearch");

            // new game initialisation
            MethodInfo NewGameResearchTargetMethod = AccessTools.Method(typeof(GameComponentUtility), "StartedNewGame");
            HarmonyMethod NewGameResearchPatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("NewGameInit"));

            harmony.Patch(NewGameResearchTargetMethod, null, NewGameResearchPatchMethod);

            // load game init
            MethodInfo LoadGameResearchTargetMethod = AccessTools.Method(typeof(GameComponentUtility), "LoadedGame");
            HarmonyMethod LoadGameResearchPatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("LoadGameInit"));

            harmony.Patch(LoadGameResearchTargetMethod, null, LoadGameResearchPatchMethod);

            // Datadisk analyzer float menu
            MethodInfo targetMethod = AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders");
            HarmonyMethod LorePostfixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddComputerDropDownMenu"));

            harmony.Patch(targetMethod, null, LorePostfixMethod);

            // research graph UI update
            MethodInfo researchTargetMethod = AccessTools.Method(typeof(MainTabWindow_Research), "DrawRightRect");
            HarmonyMethod researchPrefixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("UpdateResearchGraph"));

            harmony.Patch(researchTargetMethod, researchPrefixMethod, null);

            // patch of bill notify because currently unable to use overriden version on Experiment
            harmony.Patch(AccessTools.Method(typeof(Bill_Production),
                "Notify_IterationCompleted"), new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("NotifyExperimentIsCompleted")), null);


            // update research lists if using debug buttons
            harmony.Patch(AccessTools.Method(typeof(ResearchManager), "DebugSetAllProjectsFinished"), null,
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddResearchToListIfDebugMode")));

            // sends experiment type and size to finishedexperiment when finished production
            harmony.Patch(AccessTools.Method(typeof(GenRecipe), "MakeRecipeProducts"), null,
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("SetFinishedExpOnMake")));

            // sets a pawn as a colony researcher and adds experiment to list of authored products
            harmony.Patch(AccessTools.Method(typeof(GenRecipe), "PostProcessProduct"), null,
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("SetThingFromBill")));


            //unused methods for now 

            // drop pod event update              
            // datadisk on mechanoid disassemble
            //harmony.Patch(AccessTools.Method(typeof(Thing), "ButcherProducts"),
            //    new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddDataDiskToMechanoidLoot")),
            //    null);
                

        }

        // postfix patch of 'StartedNewGame'.
        // Initialise our undiscovered research list and remove undiscovereds from the main list.
        public static void NewGameInit()
        {
            UndiscoveredResearchList = Current.Game.GetComponent<GameComponent_ImmersiveResearch>().MainResearchDict;

            UndiscoveredResearchList.MainResearchDict.Clear();
            foreach (ResearchProjectDef proj in FullConcreteResearchList)
            {
                if(UndiscoveredResearchList.MainResearchDict.ContainsKey(proj.defName))
                {
                    //Log.Error("already contains key: " + proj.defName);
                    continue;
                }
                if (proj.IsFinished)
                {
                    if (proj.HasModExtension<ResearchDefModExtension>())
                    {
                        UndiscoveredResearchList.MainResearchDict.Add(proj.defName,
                            new ImmersiveResearchProject(proj));
                    }
                    else
                    {
                        List<ResearchTypes> list = new List<ResearchTypes>();
                        list.Add(ResearchTypes.Mod);
                        UndiscoveredResearchList.MainResearchDict.Add(proj.defName, new ImmersiveResearchProject(proj));
                    }
                }
                else
                {
                    if (proj.HasModExtension<ResearchDefModExtension>())
                    {
                        UndiscoveredResearchList.MainResearchDict.Add(proj.defName,
                            new ImmersiveResearchProject(proj));
                    }
                    else
                    {
                        List<ResearchTypes> list2 = new List<ResearchTypes>();
                        list2.Add(ResearchTypes.Mod);
                        UndiscoveredResearchList.MainResearchDict.Add(proj.defName, new ImmersiveResearchProject(proj));
                    }
                }
            }
            // TODO Figure out why adding proj specific vals in constructor only works on some of the whole dictionary
            foreach(ResearchProjectDef proj in FullConcreteResearchList)
            {
                if (proj.IsFinished)
                {
                    UndiscoveredResearchList.MainResearchDict[proj.defName].IsDiscovered = true;
                }
                else
                {
                    UndiscoveredResearchList.MainResearchDict[proj.defName].IsDiscovered = false;
                }

                if (proj.HasModExtension<ResearchDefModExtension>())
                {
                    UndiscoveredResearchList.MainResearchDict[proj.defName].Weighting = 0.0f;
                    UndiscoveredResearchList.MainResearchDict[proj.defName].ResearchTypes = proj.GetModExtension<ResearchDefModExtension>().researchTypes;
                    UndiscoveredResearchList.MainResearchDict[proj.defName].ResearchSize = proj.GetModExtension<ResearchDefModExtension>().ResearchSize;
                }
                else
                {
                    List<ResearchTypes> list2 = new List<ResearchTypes>();
                    list2.Add(ResearchTypes.Mod);
                    UndiscoveredResearchList.MainResearchDict[proj.defName].Weighting = 0.0f;
                    UndiscoveredResearchList.MainResearchDict[proj.defName].ResearchTypes = list2;
                    UndiscoveredResearchList.MainResearchDict[proj.defName].ResearchSize = ResearchSizes.Unknown;
                }
            }

            EmptyResearchGraphOfUndiscovered(DefDatabase<ResearchProjectDef>.AllDefsListForReading);
            GenerateAllBaseResearchWeightings();

            // maybe improve tutorial ? 
            Find.LetterStack.ReceiveLetter("Immersive Research - Datadisks","Immersive Research - Datadisks \n\nDatadisks are one way of obtaining new research projects and silver.\n\nYou can find datadisks by disassembling mechanoids, and from exotic traders.\n\nSome datadisks have interesting contents, click on them to see what they contain!", LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter("Immersive Research - Experiments", "Immersive Research - Experiments \n\nExperiments are your main way of obtaining new research projects. You may notice that your research window is slightly 'empty'. Don't worry, it is intended with this mod!\n\nExperiments have categorised projects into two categories; 'size' and 'type'. You can choose various combinations to be able to pinpoint which kind of research project you wish to unlock.\n\nTo make this process more bearable, you are able to see how many projects of certain combinations you are able to unlock.", LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter("Immersive Research - Studying", "Immersive Research - Studying \n\nAn important feature of this mod is a rudimentary education system. With a Study Table built, colonists will be able study finished experiments and be designated as a 'researcher'.\n\nColonists that are designated as researchers are extremely important, as they carry the colony's scientific knowledge. If your researchers die, you can lose progress on projects that they have knowledge of, even downright losing a project completely.\n\n The more researchers you have that have studied the same experiment, the less progress you will lose on that project. \n\nStudying experiments of research projects you have not discovered will discover them for you.", LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter("Immersive Research - Filing Cabinet", "Immersive Research - Filing Cabinet \n\nTo make the process of storing experiments easier, you can construct a Filing Cabinet. This will store all of your finished experiments, and you will be able to retrieve specific experiments at any time.\n\n Be careful though, as losing the cabinet can mean the loss of all of your stored experiments!", LetterDefOf.NeutralEvent);
        }


        // postfix patch of 'LoadedGame'
        // Load our undiscovered research list from file and remove undiscovereds from the main list.
        public static void LoadGameInit()
        {
            UndiscoveredResearchList = Current.Game.GetComponent<GameComponent_ImmersiveResearch>().MainResearchDict;
            //loading r Types here until i can figure out a scribing workaround for nested lists

            foreach (ResearchProjectDef proj in FullConcreteResearchList)
            {
                if (proj.HasModExtension<ResearchDefModExtension>())
                {
                    UndiscoveredResearchList.MainResearchDict[proj.defName].ResearchTypes = proj.GetModExtension<ResearchDefModExtension>().researchTypes;
                    UndiscoveredResearchList.MainResearchDict[proj.defName].ResearchSize = proj.GetModExtension<ResearchDefModExtension>().ResearchSize;
                }
                else
                {

                    List<ResearchTypes> temp = new List<ResearchTypes>();
                    temp.Add(ResearchTypes.Mod);
                    UndiscoveredResearchList.MainResearchDict[proj.defName].ResearchTypes = new List<ResearchTypes>(temp);
                    UndiscoveredResearchList.MainResearchDict[proj.defName].ResearchSize = ResearchSizes.Unknown;
                }
            }

            EmptyResearchGraphOfUndiscovered(DefDatabase<ResearchProjectDef>.AllDefsListForReading);
            GenerateAllBaseResearchWeightings();
        }


        #region UTIL FUNCTIONS

        public static List<Thing> GetAllOfThingsOnMap(string defName)
        {
            List<Thing> result = null;

            if (!(from t in DefDatabase<ThingDef>.AllDefs where t.defName == defName select t).TryRandomElement(out ThingDef finalDef))
            {
                Log.Error("Unable to locate " + defName + " in DefDatabase.", false);
            }
            else
            {
                var def = finalDef;
                var req = ThingRequest.ForDef(def);
                var thingList = new List<Thing>();
                thingList = Find.CurrentMap.listerThings.ThingsMatching(req);

                if (thingList.Count == 0)
                {
                    //Log.Error("No things found in colony.");
                    return null;
                }
                result = thingList;
               // Log.Error("Num of found things in colony: " + thingList.Count);
            }
            return result;
        }


        // TODO: figure out different way to call derived notify instead of using toils
        // currently using this since cannot reliably call notify from workgiver
        public static void NotifyExperimentIsCompleted(Bill_Production __instance, ref Pawn billDoer)
        {
            Building_ExperimentBench expBench = __instance.billStack.billGiver.LabelShort == "Experiment Bench"? expBench = (Building_ExperimentBench)__instance.billStack.billGiver: null;
            Building_StudyTable studyTable = __instance.billStack.billGiver.LabelShort == "Study Table" ? studyTable = (Building_StudyTable)__instance.billStack.billGiver : null;

            if (expBench!=null)
            {
                int index = expBench.ExpStack.IndexOfBillToExp(__instance);
                Experiment Exp = expBench.ExpStack.Experiments[index];
                expBench.ExpStack.Delete(Exp);
            }
            if (studyTable != null)
            {
                int index = studyTable.ExpStack.IndexOfBillToExp(__instance);
                Experiment Exp = studyTable.ExpStack.Experiments[index];
                studyTable.ExpStack.Delete(Exp);
            }
        }

        // TODO: find potential way to not have to patch this?
        //MakeRecipeProducts
        public static void SetFinishedExpOnMake(ref RecipeDef recipeDef, ref Pawn worker, ref List<Thing> ingredients)
        {
            if (recipeDef.ProducedThingDef != null)
            {              
                if (recipeDef.ProducedThingDef.defName == "FinishedExperiment")
                {               
                    // check if we are studying an experiment instead of making one
                    // if research is not discovered, discover it
                    if(recipeDef.defName == "StudyFinishedExperiment")
                    {
                        if (ingredients[0].def.GetModExtension<ResearchDefModExtension>().ResearchDefAttachedToExperiment != "")
                        {
                            var thingcomp = ingredients[0].TryGetComp<ResearchThingComp>();
                            ingredients[0].def.GetModExtension<ResearchDefModExtension>().ResearchDefAttachedToExperiment = thingcomp.researchDefName;
                            recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().ExperimentHasBeenMade = true;
                            Current.Game.GetComponent<GameComponent_ImmersiveResearch>().MainResearchDict.MainResearchDict[thingcomp.researchDefName].IsDiscovered = true;
                        }
                    }
                    else
                    {
                        recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().researchTypes.Clear();
                        recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().ResearchSize = ResearchSizes.None;
                        recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().ResearchDefAttachedToExperiment = "";
                        recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().ExperimentHasBeenMade = false;

                        recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().researchTypes.AddRange(recipeDef.GetModExtension<ResearchDefModExtension>().researchTypes);
                        recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().ResearchSize = recipeDef.GetModExtension<ResearchDefModExtension>().ResearchSize;
                    }
                }
            }

        }

        //PostProcessProduct
        public static void SetThingFromBill(ref Thing product, ref Pawn worker)
        {
            if(product.def.defName == "FinishedExperiment")
            {
                var tempComp = product.TryGetComp<ResearchThingComp>();

                if (tempComp != null)
                {
                    tempComp.AddResearch(product.def.GetModExtension<ResearchDefModExtension>().ResearchDefAttachedToExperiment);
                    tempComp.AddPawnAuthor(worker.Name.ToString());
                    Current.Game.GetComponent<GameComponent_ImmersiveResearch>().AddColonyExperimentToPawn(tempComp.pawnExperimentAuthorName, tempComp.researchDefName);

                    var tempHediff = HediffMaker.MakeHediff(ResearchHediffDefOf.ResearchHediff, worker, null);
                    worker.health.AddHediff(tempHediff);
                }
                else
                { 
                    Log.Error("Could not get ResearchComp.");
                }

            }
        }

        // patches the 'Complete all research' debug mode button.
        public static void AddResearchToListIfDebugMode()
        {
            for(int i = 0; i < FullConcreteResearchList.Count; i++)
            {
                if(UndiscoveredResearchList.MainResearchDict[FullConcreteResearchList[i].defName].IsDiscovered == false)
                {
                    AddNewResearch(FullConcreteResearchList[i].defName);
                    Find.ResearchManager.FinishProject(FullConcreteResearchList[i]);
                }
            }
        }

        public static int GetNumOfUnfoundProjsByRecipe(RecipeDef recipe)
        {
            int result = 0;

            if (recipe.HasModExtension<ResearchDefModExtension>())
            {
                var temp = UndiscoveredResearchList.MainResearchDict.Where(item => item.Value.ResearchSize == recipe.GetModExtension<ResearchDefModExtension>().ResearchSize 
                && item.Value.ResearchTypes.Any(x => x == recipe.GetModExtension<ResearchDefModExtension>().researchTypes[0]));
                
                foreach(KeyValuePair<string, ImmersiveResearchProject> p in temp)
                {
                    if(p.Value.IsDiscovered == false)
                    {
                        result++;
                    }
                }
            }

            return result;
        }

        #endregion

        #region DATADISK RELATED FUNCTIONS

        public static Thing ChooseDataDiskTypeOnDecrypt()
        {
            // choose datadisk at random weighting
            float threshold = Rand.Range(0, 100);
           
            if(threshold <= _superRareDatadiskProbability)
            {
                return CreateDataDiskThing("ValuableDatadisk");
            }
            else if (threshold <= _rareDatadiskProbability && threshold > _superRareDatadiskProbability)
            {
                return CreateDataDiskThing("ResearchDatadisk");
            }

            return CreateDataDiskThing("UselessDatadisk");
        }

        private static Thing CreateDataDiskThing(string thingDefName)
        {          
            ThingDef dataDef = null;
            if (!(from t in DefDatabase<ThingDef>.AllDefs where t.defName == thingDefName select t).TryRandomElement(out ThingDef finalDef))
            {
                Log.Error("Unable to locate Datadisk def in DefDatabase.", false);
                return null;
            }
            else
            {
                dataDef = finalDef;
            }

            Thing datadisk = ThingMaker.MakeThing(dataDef);

            return datadisk;
        }


        private static string GetModFilePath()
        {
            bool localInstalled = false;
            bool workshopInstalled = false;

            DirectoryInfo d = Directory.GetParent(Environment.CurrentDirectory);
            string temp = d.FullName;
            string relativePath = "";

            string tempWorkshopPath = Path.GetFullPath(Path.Combine(temp, @"..\workshop\\content\\294100\\1732571153"));
            string tempLocalPath = Path.GetFullPath(Path.Combine(temp, @"RimWorld\\Mods\\ImmersiveResearch"));

            if (Directory.Exists(tempLocalPath))
            {
                localInstalled = true;
                relativePath = tempLocalPath;
            }
            if (Directory.Exists(tempWorkshopPath))
            {
                workshopInstalled = true;
                relativePath = tempWorkshopPath;
            }
            // prioritise workshop path 
            if (workshopInstalled && localInstalled)
            {
                relativePath = tempWorkshopPath;
            }

            return relativePath;
        }


        public static string GenerateRandomDatadiskDescription(Thing datadisk)
        {
            XDocument descList;
            IEnumerable<string> results;
            string description = "";
            string relativePath = GetModFilePath();

            switch (datadisk.def.defName)
            {
                case "UselessDatadisk":
                    descList = XDocument.Load(relativePath + "\\DatadiskDescriptions\\UselessDatadiskDescriptions.xml");
                    results = descList.Descendants("item").Select(x => (string)x);
                    description = results.ElementAt(Rand.Range(0, results.Count()));
                    break;                  

                case "ValuableDatadisk":
                    descList = XDocument.Load(relativePath + "\\DatadiskDescriptions\\UselessDatadiskDescriptions.xml");
                    results = descList.Descendants("item").Select(x => (string)x);
                    description = results.ElementAt(Rand.Range(0, results.Count()));
                    break;
            }

            return description;
        }

        // prefix patch of Thing.ButcherProducts
        // delete this (test first)
        public static void AddDataDiskToMechanoidLoot(Thing __instance)
        {
            if (__instance.def.race.FleshType == FleshTypeDefOf.Mechanoid)
            {
                __instance.def.butcherProducts.Add(new ThingDefCountClass(CreateDataDiskThing("LockedDatadisk").def, 1));
            }
        }

        #endregion

        #region RESEARCH GRAPH RELATED FUNCTIONS

        public static void GenerateResearchWeightingsByType(ResearchTypes type, float weightFactor, List<ResearchProjectDef> projs)
        {          
            for(int i = 0; i < projs.Count; i++)
            {
                UndiscoveredResearchList.MainResearchDict[projs[i].defName].Weighting = weightFactor;
            }
        }

        public static string SelectResearchByWeightingAndType(List<ResearchProjectDef> projs)
        {// TODO improve this 
            // maybe regen old base weightings after selection
            List<ImmersiveResearchProject> tempList = new List<ImmersiveResearchProject>();

            for(int i = 0; i < projs.Count; i++)
            {
                tempList.Add(UndiscoveredResearchList.MainResearchDict[projs[i].defName]);
            }

            return AddNewResearch(SelectResearchByUniformCumulativeProb(tempList));
        }

        private static void GenerateAllBaseResearchWeightings()
        {
            for(int i = 0; i < FullConcreteResearchList.Count; i++)
            {
                var currentProj = FullConcreteResearchList[i];
                float weight = GenerateBaseResearchWeighting(currentProj);
                UndiscoveredResearchList.MainResearchDict[currentProj.defName].Weighting = weight; 
            }
        }


        private static float GenerateBaseResearchWeighting(ResearchProjectDef proj)
        {
            float projWeighting = 0.0f;

            if(UndiscoveredResearchList.MainResearchDict[proj.defName].IsDiscovered == true) { return projWeighting; }

            // give higher chance of selection for research lines that are more completed
            if (!proj.prerequisites.NullOrEmpty())
            {
                for (int i = 0; i < proj.prerequisites.Count; ++i)
                {
                    if (proj.prerequisites[i].IsFinished)
                    {
                        if (UndiscoveredResearchList.MainResearchDict[proj.prerequisites[i].defName].IsDiscovered == true)
                        {
                            projWeighting += 5.0f;
                        }
                    }
                }
            }

            switch (proj.techLevel)
            {
                case TechLevel.Neolithic:
                    projWeighting += _neolithicProbability;
                    break;
                case TechLevel.Medieval:
                    projWeighting += _medievalProbability;
                    break;
                case TechLevel.Industrial:
                    projWeighting += _industrialProbability;
                    break;
                case TechLevel.Spacer:
                    projWeighting += _spacerAndAboveProbability;
                    break;
                default:
                    break;
            }

            return projWeighting;
        }


        public static string SelectResearchByUniformCumulativeProb(List<ImmersiveResearchProject> projs)
        {
            string result = "";
            float totalWeighting = 0f;
            float finalTotal = 0f;

            foreach (var temp in projs)
            {
                totalWeighting += temp.Weighting;
            }

            float randVal = Rand.Range(0, totalWeighting);

            for (int index = 0; index < projs.Count; ++index)
            {
                finalTotal += projs[index].Weighting;
                if (finalTotal > randVal)
                {
                    //Log.Error("Final Weighting " + finalTotal.ToString());
                   // Log.Error("Research Selected " + projs[index].ProjectDef.defName);

                    result = projs[index].ProjectDef.defName;
                    break;
                }
            }

            return result;
        }

        public static string AddNewResearch(string researchName)
        {
            //Log.Error("New research name: " + researchName, false);
            Find.LetterStack.ReceiveLetter("New Research Discovered", "A new research project has been discovered. We have discovered " + researchName + ".", LetterDefOf.PositiveEvent);
            AddNewResearchToGraph(researchName);
            return researchName;
        }

        // prefix patch of ResearchWindow DrawRightRect function
        // UI update function called on the righthand research window when it is opened.
        // TODO: maybe overhaul window completely?
        public static void UpdateResearchGraph()
        {
            List<ResearchProjectDef> researchDatabaseRef = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
            // empty the research graph every time we open the research window if we need to

            if (researchDatabaseRef.Count() < FullConcreteResearchList.Count())
            {
                EmptyResearchGraphOfUndiscovered(researchDatabaseRef);
            }
            else
            {
               // Log.Error("Research Graph is Full/Something is broken.");
               // Log.Error(researchDatabaseRef.Count.ToString());
            }
        }

        // TODO: make 'undiscoveredResearchDef' and insert into DefDatabase to take place of removed research buttons
        // give the def a name of ??? and desc of some kinda undiscovered lore text
        // CANT BE DONE WITHOUT SIGNIFICANT CHANGES TO RESEARCH WINDOW: WILL PROBS REQUIRE COMPLETE OVERHAUL TO DO
        private static bool EmptyResearchGraphOfUndiscovered(List<ResearchProjectDef> Rlist)
        {

            for (int index = 0; index < Rlist.Count; ++index)
            {
                if (UndiscoveredResearchList.MainResearchDict.ContainsKey(Rlist[index].defName) && UndiscoveredResearchList.MainResearchDict[Rlist[index].defName].IsDiscovered == false)
                {
                    Rlist.RemoveAt(index);
                }
            }
            return true;
        }

        private static void AddNewResearchToGraph(string researchName)
        {
            var ResearchToAdd = FullConcreteResearchList.Where(item => item.defName == researchName);

            foreach (ResearchProjectDef proj in ResearchToAdd)
            {
                DefDatabase<ResearchProjectDef>.AllDefsListForReading.Add(proj);
            }

            // set the newly discovered research to 'discovered'
            ImmersiveResearchProject newValues = new ImmersiveResearchProject(UndiscoveredResearchList.MainResearchDict[researchName].ProjectDef, true, UndiscoveredResearchList.MainResearchDict[researchName].Weighting, UndiscoveredResearchList.MainResearchDict[researchName].ResearchTypes, UndiscoveredResearchList.MainResearchDict[researchName].ResearchSize);
            UndiscoveredResearchList.MainResearchDict[researchName] = newValues;

            //Log.Error("New research added to main graph: " + UndiscoveredResearchList.MainResearchDict[researchName], false);
        }
        #endregion

        #region WORK TOIL RELATED FUNCTIONS

        private static TargetingParameters AccessComputer()
        {
            var targetParams = new TargetingParameters
            {
                /* Dont need to use these unless you want to be extra
                 * sure youre clicking what you want to click on
                canTargetBuildings = true,
                canTargetPawns = false,
                mapObjectTargetsMustBeAutoAttackable = false,*/
                validator = x => x.Thing is Building_LoreComputer building
            };          
            return targetParams;
        }

        private static TargetingParameters AccessFilingCabinet()
        {
            var targetParams = new TargetingParameters
            {
                validator = x => x.Thing is Building_ExperimentFilingCabinet building
            };
            return targetParams;
        }

        // postfix patch of Drop down menu function
        public static void AddComputerDropDownMenu(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            DatadiskAnalyzerFloatMenu(clickPos, pawn, opts);
            FilingCabinetFloatMenu(clickPos, pawn, opts);
        }

        // TODO: VERY HACKY IMPLEMENTATION PLS FIND A WORKAROUND
        // really dont wanna use a static var to store a requested thing
        // find a way to pass params to jobdrivers
        public static Thing TempRequestedExp;

        private static void FilingCabinetFloatMenu(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            foreach (var localTargetInfo4 in GenUI.TargetsAt_NewTemp(clickPos, clickParams: AccessFilingCabinet(), thingsOnly: true))
            {
                var destination = localTargetInfo4;
                if (pawn.CanReach(destination, Verse.AI.PathEndMode.OnCell, Danger.Deadly) == false)
                {
                    opts.Add(new FloatMenuOption("Cannot Reach" + " (" + "NoPath".Translate() + ")", null));
                }
                else
                {
                    var cabinet = destination.Thing as Building_ExperimentFilingCabinet;

                    bool CheckIfAnyExperimentsStored()
                    {
                        if(cabinet.ListCount == 0)
                        {
                            return false;
                        }
                        return true;
                    }

                    void AddExperimentAction()
                    {// TODO fix null ref occuring if no exps exist, cant find actual reason why
                        if (GetAllOfThingsOnMap("FinishedExperiment")== null)
                        {
                            Messages.Message("No Finished Experiments located in colony.", cabinet, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else
                        {
                            var job = JobMaker.MakeJob(AddExperimentJobDefOf.CabinetAddExperiment, cabinet);
                            job.playerForced = true;
                            job.count = 1;//GetAllOfThingsOnMap("FinishedExperiment").Count;
                            pawn.jobs.TryTakeOrderedJob(job);                          
                        }

                    };

                    var label = "Add all experiments to cabinet";
                    var action = (Action)AddExperimentAction;
                    var priority = MenuOptionPriority.InitiateSocial;
                    var thing = destination.Thing;

                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, priority, null, thing), pawn, cabinet));


                    void TakeExperimentAction()
                    {
                        if (!CheckIfAnyExperimentsStored())
                        {
                            Messages.Message("No Finished Experiments stored in cabinet.", cabinet, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else
                        {
                            var job = JobMaker.MakeJob(TakeExperimentJobDefOf.CabinetTakeExperiment, cabinet);
                            job.playerForced = true;
                            pawn.jobs.TryTakeOrderedJob(job);                          
                        }
                    };

                    if (CheckIfAnyExperimentsStored())
                    {
                        foreach (var exp in cabinet.CabinetThings)
                        {
                            var temp = exp.Value as Thing_FinishedExperiment;
                            var label2 = "Take " + temp.TryGetComp<ResearchThingComp>().researchDefName + " Experiment";
                            var action2 = (Action)TakeExperimentAction;
                            var priority2 = MenuOptionPriority.InitiateSocial;
                            var thing2 = destination.Thing;

                            TempRequestedExp = exp.Value;

                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label2, action2, priority2, null, thing2), pawn, cabinet));
                        }
                    }
                }
            }
        }

        private static void DatadiskAnalyzerFloatMenu(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            // check against our custom target parameters that we are clicking what we need to click on
            foreach (var localTargetInfo4 in GenUI.TargetsAt_NewTemp(clickPos, clickParams: AccessComputer(), thingsOnly: true))
            {
                var destination = localTargetInfo4;
                if (pawn.CanReach(destination, Verse.AI.PathEndMode.OnCell, Danger.Deadly) == false)
                {
                    opts.Add(new FloatMenuOption("Cannot Reach" + " (" + "NoPath".Translate() + ")", null));
                }
                // Possible TODO: add extra conditionals based on pawn skill levels
                else
                {
                    var pawnTarget = (Building_LoreComputer)destination.Thing;

                    bool CheckIfAllResearchDone()
                    {
                        int maxNumOfResearches = FullConcreteResearchList.Count();
                        int counter = 1;
                        for (int i = 0; i < UndiscoveredResearchList.MainResearchDict.Count(); i++)
                        {
                            if (UndiscoveredResearchList.MainResearchDict.ElementAt(i).Value.IsDiscovered == true)
                            {
                                counter++;
                            }
                        }

                        if (counter == maxNumOfResearches)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    // create our button press local functions
                    // research disks
                    void Action4()
                    {
                        if (!pawnTarget.CheckIfLinkedToResearchBench())
                        {
                            Messages.Message("Datadisk Analyzer is not linked to a valid research bench.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        if (pawnTarget.PowerComponent.PowerOn != true)
                        {
                            Messages.Message("Datadisk Analyzer is not powered.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        if (pawnTarget.GetLocationOfOwnedThing("ResearchDatadisk") == null)
                        {
                            Messages.Message("No Research disks are owned in colony.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else if (CheckIfAllResearchDone())
                        {
                            Messages.Message("All research options are already completed.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else
                        {
                            var job = JobMaker.MakeJob(LoreResearchDiskDefOf.LoreResearchDiskDef, pawnTarget);
                            job.playerForced = true;
                            pawn.jobs.TryTakeOrderedJob(job);
                        }
                    }

                    // locked disks
                    void Action5()
                    {
                        if (pawnTarget.GetLocationOfOwnedThing("LockedDatadisk") == null)
                        {
                            Messages.Message("No Encrypted datadisks are owned in colony.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        if (!pawnTarget.CheckIfLinkedToResearchBench())
                        {
                            Messages.Message("Datadisk Analyzer is not linked to a valid research bench.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else if (pawnTarget.PowerComponent.PowerOn != true)
                        {
                            Messages.Message("Datadisk Analyzer is not powered.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else
                        {
                            var job = JobMaker.MakeJob(LoreDataDiskDefOf.LoreDataDiskDef, pawnTarget);
                            job.playerForced = true;
                            pawn.jobs.TryTakeOrderedJob(job);
                        }
                    }

                    // create the drop down menu button and functionality
                    var label = "Load Research Datadisk";
                    var action = (Action)Action4;
                    var priority = MenuOptionPriority.InitiateSocial;
                    var thing = destination.Thing;

                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, priority, null, thing), pawn, pawnTarget));

                    var label2 = "Decode Datadisk";
                    var action2 = (Action)Action5;
                    var priority2 = MenuOptionPriority.InitiateSocial;
                    var thing2 = destination.Thing;

                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label2, action2, priority2, null, thing2), pawn, pawnTarget));
                }
            }
        }

        #endregion
    }
}
