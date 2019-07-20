using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Harmony;
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
        Construction,
        Metallurgy,
        Carpentry,
        Weaponry,
        Apparel,
        Masonry,
        Electrical,
        Medical,
        Spacecraft,
        Advanced,
        Spacer,
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
    /// Class that uses Harmony to inject custom code into RimWorld code at runtime.
    /// </summary>
    [StaticConstructorOnStartup]
    static class LoreComputerHarmonyPatches
    {

        private static float _neolithicProbability = 40.0f;
        private static float _medievalProbability = 20.0f;
        private static float _industrialProbability = 10.0f;
        private static float _spacerProbability = 5.0f;
        private static float _rareDatadiskProbability = 50.0f;
        private static float _superRareDatadiskProbability = 5.0f;

        public static List<ResearchProjectDef> TempResearchList = new List<ResearchProjectDef>(DefDatabase<ResearchProjectDef>.AllDefsListForReading); // a concrete list of all possible research options

        public static ResearchDict UndiscoveredResearchList;
        
        
        public static Dictionary<int, string> DatadiskUniqueDescriptions = new Dictionary<int, string>(); // dictionary of in game datadisks unique IDs that point to randomly chosen descriptions.

        static LoreComputerHarmonyPatches()
        {          
            var harmony = HarmonyInstance.Create("rimworld.mods.immersiveresearch");

            // new game initialisation
            MethodInfo NewGameResearchTargetMethod = AccessTools.Method(typeof(GameComponentUtility), "StartedNewGame");
            HarmonyMethod NewGameResearchPatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("NewGameInit"));

            harmony.Patch(NewGameResearchTargetMethod, null, NewGameResearchPatchMethod);

            // load game init
            MethodInfo LoadGameResearchTargetMethod = AccessTools.Method(typeof(GameComponentUtility), "LoadedGame");
            HarmonyMethod LoadGameResearchPatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("LoadGameInit"));

            harmony.Patch(LoadGameResearchTargetMethod, null, LoadGameResearchPatchMethod);

            // Lore Database float menu
            MethodInfo targetMethod = AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders");
            HarmonyMethod LorePostfixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddComputerDropDownMenu"));

            harmony.Patch(targetMethod, null, LorePostfixMethod);

            // research graph UI update
            MethodInfo researchTargetMethod = AccessTools.Method(typeof(MainTabWindow_Research), "DrawRightRect");
            HarmonyMethod researchPrefixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("UpdateResearchGraph"));

            harmony.Patch(researchTargetMethod, researchPrefixMethod, null);

            // datadisk on ship trader stock
            MethodInfo miscStockTargetMethod = AccessTools.Method(typeof(TradeShip), "GenerateThings");
            HarmonyMethod miscStockPostfixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddDataDiskToStock"));

            harmony.Patch(miscStockTargetMethod, null, miscStockPostfixMethod);

            // bandit camp quest reward
            MethodInfo banditCampTargetMethod = AccessTools.Method(typeof(IncidentWorker_QuestBanditCamp), "GenerateRewards");
            HarmonyMethod bandCampPostfixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddDataDiskToBanditQuestReward"));

            harmony.Patch(banditCampTargetMethod, null, bandCampPostfixMethod);

            // datadisk on mechanoid disassemble
            harmony.Patch(AccessTools.Method(typeof(Thing), "ButcherProducts"),
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddDataDiskToMechanoidLoot")),
                null);

            // datadisk destroy changes
            harmony.Patch(AccessTools.Method(typeof(Thing), "Destroy"), null,
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("RemoveDatadiskFromDict")));

            // inspect window changes 
            harmony.Patch(AccessTools.Method(typeof(StatsReportUtility), "DescriptionEntry", new Type[] { typeof(Thing)}), null,
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("ModifyUniqueDatadiskInspectText")));

            // update research lists if using debug buttons
            harmony.Patch(AccessTools.Method(typeof(ResearchManager), "DebugSetAllProjectsFinished"), null,
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddResearchToListIfDebugMode")));

            // notify iteration completed on Bill_Production
            harmony.Patch(AccessTools.Method(typeof(Bill_Production), 
                "Notify_IterationCompleted"), new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("NotifyExperimentIsCompleted")), null);

            //TEMPORARY: set FinishedExp research vals on MakeThing
            harmony.Patch(AccessTools.Method(typeof(GenRecipe), "MakeRecipeProducts"), null,
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("SetFinishedExpOnMake")));


            //unused methods for now 

            // drop pod event update
            /*harmony.Patch(AccessTools.Method(typeof(DropPodUtility), "DropThingsNear"), 
                null, 
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddDataDiskToDropPodEvent")));
                */

        }

        // postfix patch of 'StartedNewGame'.
        // Initialise our undiscovered research list and remove undiscovereds from the main list.
        public static void NewGameInit()
        {
            UndiscoveredResearchList = Current.Game.GetComponent<GameComponent_ImmersiveResearch>().MainResearchDict;

            UndiscoveredResearchList.MainResearchDict.Clear();
            foreach (ResearchProjectDef proj in TempResearchList)
            {
                if(UndiscoveredResearchList.MainResearchDict.ContainsKey(proj.defName))
                {
                    Log.Error("already contains key: " + proj.defName);
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
            foreach(ResearchProjectDef proj in TempResearchList)
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
        }


        // postfix patch of 'LoadedGame'
        // Load our undiscovered research list from file and remove undiscovereds from the main list.
        public static void LoadGameInit()
        {
            UndiscoveredResearchList = Current.Game.GetComponent<GameComponent_ImmersiveResearch>().MainResearchDict;
            //loading r Types here until i can figure out a scribing workaround for nested lists

            foreach (ResearchProjectDef proj in TempResearchList)
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

        // probable TODO: change Experiment to inherit from Bill so I can override Notify Function?
        public static void NotifyExperimentIsCompleted(Bill_Production __instance, ref Pawn billDoer)
        {
            if(__instance.billStack.billGiver.LabelShort == "Experiment Bench")
            {
                // delete the experiment instance from expStack when Bill is completed
                Building_ExperimentBench bench = (Building_ExperimentBench)__instance.billStack.billGiver;
                int index = bench.ExpStack.IndexOfBillToExp(billDoer.CurJob.bill);
                Experiment Exp = bench.ExpStack[index];
                bench.ExpStack.Delete(Exp);
            }
        }

        // Temp solution until i figure out how to send recipeDef vals to the Thing Objects
        public static void SetFinishedExpOnMake(ref RecipeDef recipeDef)
        {
            if(recipeDef.ProducedThingDef.defName == "FinishedExperiment")
            {
                recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().researchTypes.Clear();
                recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().ResearchSize = ResearchSizes.None;
                recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().researchTypes.AddRange(recipeDef.GetModExtension<ResearchDefModExtension>().researchTypes);
                recipeDef.ProducedThingDef.GetModExtension<ResearchDefModExtension>().ResearchSize = recipeDef.GetModExtension<ResearchDefModExtension>().ResearchSize;
            }
        }

        // patches the 'Complete all research' debug mode button.
        public static void AddResearchToListIfDebugMode()
        {
            for(int i = 0; i < TempResearchList.Count; i++)
            {
                if(UndiscoveredResearchList.MainResearchDict[TempResearchList[i].defName].IsDiscovered == false)
                {
                    AddNewResearch(TempResearchList[i].defName);
                    Find.ResearchManager.FinishProject(TempResearchList[i]);
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

        //postfix patch of Thing.Destroy
        public static void RemoveDatadiskFromDict(Thing __instance)
        {
            if(__instance.def.defName == "UselessDatadisk" || __instance.def.defName == "ValuableDatadisk")
            {
                Log.Error("destruction ID " + __instance.thingIDNumber.ToString());
                DatadiskUniqueDescriptions.Remove(__instance.thingIDNumber-2);
            }
        }

        // postfix patch of Inspection window text generation
        public static void ModifyUniqueDatadiskInspectText(ref Thing thing, StatDrawEntry __result)
        {
            if (!thing.DestroyedOrNull())
            {
                //Log.Error("thing ID: " + thing.thingIDNumber);
                if (thing.def.defName == "UselessDatadisk" || thing.def.defName == "ValuableDatadisk")
                {
                    if(DatadiskUniqueDescriptions.ContainsKey(thing.thingIDNumber-2))
                    {
                        //Log.Error("desc edit ID " + thing.thingIDNumber.ToString());
                        //Log.Error(DatadiskUniqueDescriptions[thing.thingIDNumber - 2]);
                        __result.overrideReportText = DatadiskUniqueDescriptions[thing.thingIDNumber - 2];
                    }
                    else
                    {
                        __result.overrideReportText = "stop cheating";
                    }
                }
            }
        }


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
            // appears that Thing unique ID is offset by +2 at some point after creation
            Thing datadisk = ThingMaker.MakeThing(dataDef);

            if (datadisk.def.defName != "LockedDatadisk" && datadisk.def.defName != "ResearchDataDisk")
            {
                DatadiskUniqueDescriptions.Add(datadisk.thingIDNumber, datadisk.def.description + " " + GenerateRandomDatadiskDescription(datadisk));                
            }
                            
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


        private static string GenerateRandomDatadiskDescription(Thing datadisk)
        {
            // generate a random description from xml files depending on datadisk type.
            // we also need to check whether mod is installed locally or through steam workshop.
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
        public static void AddDataDiskToMechanoidLoot(Thing __instance)
        {
            if (__instance.def.race.FleshType == FleshTypeDefOf.Mechanoid)
            {
                __instance.def.butcherProducts.Add(new ThingDefCountClass(CreateDataDiskThing("LockedDatadisk").def, 1));
            }
        }

        // postfix patch of TradeShip.GenerateThings
        public static void AddDatadiskToStock(TradeShip __instance)
        {
            // not sure if we need to edit ship stocks as they are auto added to stocks via XML tags
            __instance.Goods.Add<Thing>(CreateDataDiskThing("LockedDatadisk"));
        }

        // postfix patch of Bandit camp reward generation
        public static void AddDataDiskToBanditQuestReward(List<Thing> __result)
        {
            __result.Add(CreateDataDiskThing("LockedDatadisk"));
        }

        
        public static void AddDataDiskToDropPodEvent(ref IEnumerable<Thing> things)
        {
            things.Add(CreateDataDiskThing("LockedDatadisk"));
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

        public static void SelectResearchByWeightingAndType(List<ResearchProjectDef> projs)
        {// TODO improve this 
            // maybe regen old base weightings after selection
            List<ImmersiveResearchProject> tempList = new List<ImmersiveResearchProject>();

            for(int i = 0; i < projs.Count; i++)
            {
                tempList.Add(UndiscoveredResearchList.MainResearchDict[projs[i].defName]);
            }

            AddNewResearch(SelectResearchByUniformCumulativeProb(tempList));
        }

        private static void GenerateAllBaseResearchWeightings()
        {
            for(int i = 0; i < TempResearchList.Count; i++)
            {
                ResearchProjectDef currentProj = TempResearchList[i];
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
                    projWeighting += _spacerProbability;
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
                    //Log.Error("Research Selected " + projs[index].ProjectDef.defName);
                    result = projs[index].ProjectDef.defName;
                    break;
                }
            }

            return result;
        }


        public static void ApplyBlindResearch(ref ResearchProjectDef proj)
        {// unused atm
            if(proj.defName == "BlindResearch")
            {
                //SelectItemByUniformCumulativeProb();
                proj.baseCost += 500f;
            }
        }


        public static void AddNewResearch(string researchName)
        {
            //Log.Error("New research name: " + researchName, false);
            Find.LetterStack.ReceiveLetter("New Research Discovered", "A new research project has been discovered. We have discovered " + researchName + ".", LetterDefOf.PositiveEvent);
            FillResearchGraph(researchName);
        }

        // prefix patch of ResearchWindow DrawRightRect function
        // UI update function called on the righthand research window when it is opened.
        public static void UpdateResearchGraph()
        {
            List<ResearchProjectDef> researchDatabaseRef = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
            // empty the research graph every time we open the research window if we need to

            if (researchDatabaseRef.Count() < TempResearchList.Count())
            {
                EmptyResearchGraphOfUndiscovered(researchDatabaseRef);
            }
            else
            {
               // Log.Error("Research Graph is Full/Something is broken.");
               // Log.Error(researchDatabaseRef.Count.ToString());
            }
        }


        private static bool EmptyResearchGraphOfUndiscovered(List<ResearchProjectDef> Rlist)
        {

            for (int index = 0; index < Rlist.Count; ++index)
            {
                ResearchProjectDef researchProjectDef = Rlist[index];

                if (UndiscoveredResearchList.MainResearchDict.ContainsKey(Rlist[index].defName) && UndiscoveredResearchList.MainResearchDict[Rlist[index].defName].IsDiscovered == false)
                {
                    //Log.Error("remving from list");
                    Rlist.RemoveAt(index);
                }
            }
            return true;
        }

        private static void FillResearchGraph(string researchName)
        {
            var ResearchToAdd = TempResearchList.Where(item => item.defName == researchName);

            foreach (ResearchProjectDef proj in ResearchToAdd)
            {
                DefDatabase<ResearchProjectDef>.AllDefsListForReading.Add(proj);
            }

            // set the newly discovered research to 'discovered'
            ImmersiveResearchProject newValues = new ImmersiveResearchProject(UndiscoveredResearchList.MainResearchDict[researchName].ProjectDef, true, UndiscoveredResearchList.MainResearchDict[researchName].Weighting, UndiscoveredResearchList.MainResearchDict[researchName].ResearchTypes, UndiscoveredResearchList.MainResearchDict[researchName].ResearchSize);
            UndiscoveredResearchList.MainResearchDict[researchName] = newValues;

            //Log.Error("Is new research discovered: " + UndiscoveredResearchList.MainResearchDict[researchName], false);
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

        // postfix patch of Drop down menu function
        // Creates a drop down menu when you right click on a specific target.
        public static void AddComputerDropDownMenu(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            // check against our custom target parameters that we are clicking what we need to click on
            foreach (var localTargetInfo4 in GenUI.TargetsAt(clickPos, AccessComputer(), true))
            {
                var destination = localTargetInfo4;
                if(pawn.CanReach(destination, Verse.AI.PathEndMode.OnCell, Danger.Deadly) == false)
                {
                    opts.Add(new FloatMenuOption("Cannot Reach" + " (" + "NoPath".Translate() + ")", null));
                }
                // maybe add else if if pawn research skill is too low (i.e. too dumb)
                else
                {
                    var pawnTarget = (Building_LoreComputer)destination.Thing;

                    bool CheckIfAllResearchDone()
                    {
                        int maxNumOfResearches = TempResearchList.Count();
                        int counter = 1;
                        for (int i = 0; i < UndiscoveredResearchList.MainResearchDict.Count(); i++)
                        {
                            if(UndiscoveredResearchList.MainResearchDict.ElementAt(i).Value.IsDiscovered == true)
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
                            Messages.Message("No Research Disks are owned in colony.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else if (CheckIfAllResearchDone())
                        {
                            Messages.Message("All Research options are already completed.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else
                        {
                            var job = new Job(LoreResearchDiskDefOf.LoreResearchDiskDef, pawnTarget);
                            job.playerForced = true;
                            pawn.jobs.TryTakeOrderedJob(job);
                        }                       
                    }

                    // locked disks
                    void Action5()
                    {
                        if (pawnTarget.GetLocationOfOwnedThing("LockedDatadisk") == null)
                        {
                            Messages.Message("No Encrypted Datadisks are owned in colony.", pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
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
                            var job = new Job(LoreDataDiskDefOf.LoreDataDiskDef, pawnTarget);
                            job.playerForced = true;
                            pawn.jobs.TryTakeOrderedJob(job);
                        }
                    }

                    // create the drop down menu button and functionality
                    var label = "Load Research Disk";
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
