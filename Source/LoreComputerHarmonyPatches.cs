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
    // Rimworld is built in .Net3.5 which does not have Tuples
    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }

        internal Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
    public static class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 Item1, T2 Item2)
        {
            var tuple = new Tuple<T1, T2>(Item1, Item2);
            return tuple;
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
        private static float _rareDatadiskProbability = 20.0f;
        private static float _superRareDatadiskProbability = 5.0f;

        private static string _currentSaveName = "";

        public static List<ResearchProjectDef> TempResearchList = new List<ResearchProjectDef>(DefDatabase<ResearchProjectDef>.AllDefsListForReading); // a concrete list of all possible research options

        public static Dictionary<string, Tuple<bool, float>> UndiscoveredResearchList = new Dictionary<string, Tuple<bool, float>>();                  // dictionary of all possible research options, with respective 'discovered' flags and weightings.

        public static Dictionary<int, string> DatadiskUniqueDescriptions = new Dictionary<int, string>();                                              // dictionary of in game datadisks unique IDs that point to randomly chosen descriptions.

        static LoreComputerHarmonyPatches()
        {
            
            var harmony = HarmonyInstance.Create("rimworld.mods.lorefriendlywiki");

            // new game initialisation
            MethodInfo NewGameResearchTargetMethod = AccessTools.Method(typeof(GameComponentUtility), "StartedNewGame");
            HarmonyMethod NewGameResearchPatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("NewGameInit"));

            harmony.Patch(NewGameResearchTargetMethod, null, NewGameResearchPatchMethod);

            // load game init
            MethodInfo LoadGameResearchTargetMethod = AccessTools.Method(typeof(GameComponentUtility), "LoadedGame");
            HarmonyMethod LoadGameResearchPatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("LoadGameInit"));

            harmony.Patch(LoadGameResearchTargetMethod, null, LoadGameResearchPatchMethod);

            // Save game patch
            MethodInfo SaveGameTargetMethod = AccessTools.Method(typeof(SafeSaver), "Save");
            HarmonyMethod SaveGamePatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("SaveGameUtility"));

            harmony.Patch(SaveGameTargetMethod, null, SaveGamePatchMethod);

            //save game name grab
            MethodInfo SaveNameTargetMethod = AccessTools.Method(typeof(GameDataSaveLoader), "SaveGame");
            HarmonyMethod SaveNamePatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("GetSelectedSaveName"));

            harmony.Patch(SaveNameTargetMethod, SaveNamePatchMethod, null);

            // load game name grab
            MethodInfo LoadNameTargetMethod = AccessTools.Method(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow");
            HarmonyMethod LoadNamePatchMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("GetSelectedSaveName"));

            harmony.Patch(LoadNameTargetMethod, LoadNamePatchMethod, null);

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
            HarmonyMethod miscStockPostfixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddDatadiskToStock"));

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


            //unused methods for now 

            // Blind Research option functionality
            // MethodInfo researchFinishTargetMethod = AccessTools.Method(typeof(ResearchManager), "FinishProject");
            // HarmonyMethod researchFinishPrefixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("ApplyBlindResearch"));

            // harmony.Patch(researchFinishTargetMethod, researchFinishPrefixMethod, null);

            // research graph Left side of window changing
            //MethodInfo projPrereqTargetMethod = AccessTools.Method(typeof(MainTabWindow_Research), "DrawResearchPrereqs");
            //HarmonyMethod projPreqreqPostfixMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("EditResearchPrereqs"));

            //harmony.Patch(projPrereqTargetMethod, projPreqreqPostfixMethod, null);

            // drop pod event update
            /*harmony.Patch(AccessTools.Method(typeof(DropPodUtility), "DropThingsNear"), 
                null, 
                new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("AddDataDiskToDropPodEvent")));
                */

            // random research event
            /*MethodInfo researchRandomEventTargetMethod = AccessTools.Method(typeof(ResearchManager), "ResearchPerformed");
            HarmonyMethod researchRandomEventMethod = new HarmonyMethod(typeof(LoreComputerHarmonyPatches).GetMethod("ResearchRandomEvent"));

            harmony.Patch(researchRandomEventTargetMethod, researchRandomEventMethod, null);*/
        }

        // postfix patch of 'StartedNewGame'.
        // Initialise our undiscovered research list and remove undiscovereds from the main list.
        public static void NewGameInit()
        {
            UndiscoveredResearchList.Clear();
            foreach (ResearchProjectDef proj in TempResearchList)
            {
                //if(proj.defName == "BlindResearch") { continue; }
                if (proj.IsFinished)
                {
                    UndiscoveredResearchList.Add(proj.defName, new Tuple<bool, float>(true, 0.0f));
                }
                else
                {
                    UndiscoveredResearchList.Add(proj.defName, new Tuple<bool, float>(false, 0.0f));
                }
            }
            EmptyResearchGraphOfUndiscovered(DefDatabase<ResearchProjectDef>.AllDefsListForReading);
            GenerateAllResearchWeightings();
        }


        // postfix patch of 'LoadedGame'
        // Load our undiscovered research list from file and remove undiscovereds from the main list.
        public static void LoadGameInit()
        {
            UndiscoveredResearchList.Clear();

            //afaik rimworld save names can never be duplicate as entering the same save name as another save just overrides the existing one
            // so we should not need to check for duplicate xml files cause there theoretically should never BE any duplicated files
            // load research projects that are fully completed.
            // and reload previous undiscovered research data
            Dictionary<string, bool> tempDict = LoadResearchDetailsFromFile(_currentSaveName);

            if(tempDict == null)
            {
                Log.Error("something went wrong with research details retrieval. Defaulting to vanilla research graph.");
                foreach (ResearchProjectDef proj in TempResearchList)
                {
                    UndiscoveredResearchList.Add(proj.defName, new Tuple<bool, float>(true, 0.0f));
                }
                return;
            }

            // get dicovered but uncompleted
            for(int i = 0; i < tempDict.Count(); i++)
            {
                if (tempDict.ElementAt(i).Value == true)
                {
                    UndiscoveredResearchList.Add(tempDict.ElementAt(i).Key, new Tuple<bool, float>(true, 0.0f));
                }
                else
                {
                    UndiscoveredResearchList.Add(tempDict.ElementAt(i).Key, new Tuple<bool, float>(false, 0.0f));
                }
            }
            // could also do with saving weightings ( wouldnt need to unless weightings became more randomised)
            EmptyResearchGraphOfUndiscovered(DefDatabase<ResearchProjectDef>.AllDefsListForReading);
            GenerateAllResearchWeightings();
        }


        #region UTIL FUNCTIONS

        private static Dictionary<string, bool> LoadResearchDetailsFromFile(string saveName)
        {
            XDocument temp = null;
            try
            {
                temp = XDocument.Load(GetModFilePath()  + @"\\" + saveName + "ResearchDetails.xml");
            }
            catch
            {
                Log.Error("could not find research details file for this save file. Defaulting...");
                return null;
            }
            
            var rootNodes = temp.Root.DescendantsAndSelf("ResearchDetails").Elements();
            
            var researchProjs = rootNodes.ToDictionary(n => n.Name.ToString(), n => n.Value);
            Dictionary<string, bool> newDict = new Dictionary<string, bool>();

            // have to convert from xml boolean to C# version
            foreach (var item in researchProjs)
            {
                char c = char.ToUpper(item.Value[0]);
                item.Value.Replace(item.Value[0], c);
                newDict.Add(item.Key, Convert.ToBoolean(item.Value));
            }

            return newDict;
        }

        // prefix patch of 'SaveGame'.
        public static void GetSelectedSaveName(ref string fileName)
        {
            _currentSaveName = fileName;
        }

        // prefix patch of 'Save'.
        public static void SaveGameUtility()
        {
            SaveResearchDetailsToFile();
            SaveDiskDescriptionsToFile();
        }

        
        private static void SaveResearchDetailsToFile()
        {
            Dictionary<string, bool> savFileDict = new Dictionary<string, bool>();
            for (int i = 0; i < UndiscoveredResearchList.Count(); i++)
            {
                savFileDict.Add(UndiscoveredResearchList.ElementAt(i).Key, UndiscoveredResearchList.ElementAt(i).Value.Item1);
            }
         
            XElement saveFileName = new XElement("SaveName", _currentSaveName);
            XElement researchDetails = new XElement("ResearchDetails", from keyValue in savFileDict select new XElement(keyValue.Key.Replace(" ", string.Empty), keyValue.Value));

            XDocument root = new XDocument(new XElement("root", saveFileName, researchDetails));

            root.Save(GetModFilePath() + @"\\" + _currentSaveName + "ResearchDetails.xml");
        }


        private static void SaveDiskDescriptionsToFile()
        {
            if(DatadiskUniqueDescriptions.Count() <= 0)
            {
                return;
            }

            Dictionary<int, string> descriptionDict = new Dictionary<int, string>();
            for (int i = 0; i < DatadiskUniqueDescriptions.Count(); i++)
            {
                descriptionDict.Add(DatadiskUniqueDescriptions.ElementAt(i).Key, DatadiskUniqueDescriptions.ElementAt(i).Value);
            }

            XElement saveFileName = new XElement("SaveName", _currentSaveName);
            XElement descriptionDetails = new XElement("DescriptionDetails", from keyValue in descriptionDict select new XElement("T" + keyValue.Key.ToString(), keyValue.Value));

            XDocument root = new XDocument(new XElement("root", saveFileName, descriptionDetails));

            descriptionDetails.Save(GetModFilePath() + @"\\" + _currentSaveName + "DescriptionDetails.xml");
        }

        // patches the 'Complete all research' debug mode button.
        public static void AddResearchToListIfDebugMode()
        {
            for(int i = 0; i < UndiscoveredResearchList.Count(); i++)
            {
                if(UndiscoveredResearchList.ElementAt(i).Value.Item1 == false)
                {
                    AddNewResearch(UndiscoveredResearchList.ElementAt(i).Key);
                    for(int j = 0; j < TempResearchList.Count(); j++)
                    {
                        if(TempResearchList[j].label == UndiscoveredResearchList.ElementAt(i).Key)
                        {
                            Find.ResearchManager.FinishProject(TempResearchList[j]);
                        }
                    }
                }
            }
        }


        public static void ResearchRandomEvent(ref float amount, ref Pawn researcher)
        {// unused for now
            int researcherLevel = researcher.skills.GetSkill(SkillDefOf.Intellectual).Level;

            Log.Error(researcherLevel.ToString(), false);
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
            else if(threshold <= _rareDatadiskProbability && threshold > _superRareDatadiskProbability)
            {
                CreateDataDiskThing("ResearchDatadisk");
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
                __instance.def.butcherProducts.Add(new ThingDefCountClass(CreateDataDiskThing("EncryptedDatadisk").def, 1));
            }
        }

        // postfix patch of TradeShip.GenerateThings
        public static void AddDatadiskToStock(TradeShip __instance)
        {
            // not sure if we need to edit ship stocks as they are auto added to stocks via XML tags
            __instance.Goods.Add<Thing>(CreateDataDiskThing("EncryptedDatadisk"));
        }

        // postfix patch of Bandit camp reward generation
        public static void AddDataDiskToBanditQuestReward(List<Thing> __result)
        {
            __result.Add(CreateDataDiskThing("EncryptedDatadisk"));
        }

        
        public static void AddDataDiskToDropPodEvent(ref IEnumerable<Thing> things)
        {
            things.Add(CreateDataDiskThing("EncryptedDatadisk"));
        }

        #endregion

        #region RESEARCH GRAPH RELATED FUNCTIONS


        private static void GenerateAllResearchWeightings()
        {
            var undiscoveredResearch = UndiscoveredResearchList.Where(item => item.Value.Item1 == false);

            List<string> tempList = UndiscoveredResearchList.Keys.ToList();
            // you can't make changes to a dict that you are iterating over
            for (int i = 0; i < tempList.Count; ++i)
            {
                var temp = TempResearchList.Where(item => item.defName == UndiscoveredResearchList.ElementAt(i).Key);
                foreach (ResearchProjectDef currentProj in temp)
                {
                    float weight = GenerateUniqueResearchWeighting(currentProj);
                    Tuple<bool, float> newValues = new Tuple<bool, float>(UndiscoveredResearchList[currentProj.defName].Item1, weight);
                    UndiscoveredResearchList[currentProj.defName] = newValues;
                }
            }
        }


        private static float GenerateUniqueResearchWeighting(ResearchProjectDef proj)
        {
            float projWeighting = 0.0f;

            if(UndiscoveredResearchList[proj.defName].Item1 == true) { return projWeighting; }

            // give higher chance of selection for research lines that are more completed
            if (!proj.prerequisites.NullOrEmpty())
            {
                for (int i = 0; i < proj.prerequisites.Count; ++i)
                {
                    if (UndiscoveredResearchList[proj.prerequisites[i].defName].Item1 == true)
                    {
                        projWeighting += 5.0f;
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


        public static void SelectResearchByWeighting()
        {
            float totalWeighting = 0f;
            float finalTotal = 0f;
            Log.Error("selecting");
            foreach (var temp in UndiscoveredResearchList)
            {
                totalWeighting += temp.Value.Item2;
            }

            float randVal = Rand.Range(0, totalWeighting);

            for (int index = 0; index < UndiscoveredResearchList.Count; ++index)
            {
                finalTotal += UndiscoveredResearchList.ElementAt(index).Value.Item2;
                if (finalTotal > randVal)
                {
                    //Log.Error("Final Weighting " + finalTotal.ToString());
                    //Log.Error("Research Selected " + UndiscoveredResearchList.ElementAt(index).Key);
                    AddNewResearch(UndiscoveredResearchList.ElementAt(index).Key);
                    break;
                }
            }
        }


        public static void ApplyBlindResearch(ref ResearchProjectDef proj)
        {// unused atm
            if(proj.defName == "BlindResearch")
            {
                SelectResearchByWeighting();
                proj.baseCost += 500f;
            }
        }


        private static void AddNewResearch(string researchName)
        {
            //Log.Error("New research name: " + researchName, false);
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
        }


        private static bool EmptyResearchGraphOfUndiscovered(List<ResearchProjectDef> Rlist)
        {

            for (int index = 0; index < Rlist.Count; ++index)
            {
                ResearchProjectDef researchProjectDef = Rlist[index];

                if (UndiscoveredResearchList.ContainsKey(Rlist[index].defName) && UndiscoveredResearchList[Rlist[index].defName].Item1 == false)
                {
                    //Log.Error("remving from list");
                    Rlist.RemoveAt(index);
                }
            }
            return true;
        }


        public static void EditResearchPrereqs( ref ResearchProjectDef project, ref Rect rect)
        {
            // We use a bool return type harmony method here to choose whether to miss the execution of the original
            // method.

            //unused at the moment
            /*ResearchProjectDef currentProj = project;

            if(currentProj.label == "Blind Research") { return true; }

            if (UndiscoveredResearchList[currentProj.label].Item1 == true && !currentProj.prerequisites.NullOrEmpty())
            {
                // check that all prerequisite research on selected research are completed.
                int numOfDiscoveredPrereqs = 0;
                for (int i = 0; i < currentProj.prerequisites.Count; ++i)
                {
                    if (UndiscoveredResearchList[currentProj.prerequisites[i].label].Item1 == true)
                    {
                        numOfDiscoveredPrereqs++;
                    }
                }
                Log.Error(numOfDiscoveredPrereqs.ToString(), false);
                if (numOfDiscoveredPrereqs == currentProj.prerequisites.Count)
                {
                    return true;
                }
                else
                {
                    Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":");
                    rect.yMin += rect.height;
                    for (int index = 0; index < currentProj.prerequisites.Count; ++index)
                    {
                        if (UndiscoveredResearchList[currentProj.prerequisites[index].label].Item1 == false)
                        {
                            Widgets.LabelCacheHeight(ref rect, "  " + "Unknown", true, false);
                            rect.yMin += rect.height;
                        }
                        else
                        {
                            Widgets.LabelCacheHeight(ref rect, "  " + project.prerequisites[index].LabelCap);
                            rect.yMin += rect.height;
                        }
                    }
                }
                return false;
            }
            else
            {
                return true;
            }*/
        }


        private static void FillResearchGraph(string researchName)
        {
            var ResearchToAdd = TempResearchList.Where(item => item.defName == researchName);

            foreach (ResearchProjectDef proj in ResearchToAdd)
            {
                DefDatabase<ResearchProjectDef>.AllDefsListForReading.Add(proj);
            }

            // set the newly discovered research to 'discovered'
            Tuple<bool, float> newValues = new Tuple<bool, float>(true, UndiscoveredResearchList[researchName].Item2);
            UndiscoveredResearchList[researchName] = newValues;

            //Log.Error("Is new research discovered: " + UndiscoveredResearchList[researchName].Item1.ToString(), false);
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
                        for (int i = 0; i < UndiscoveredResearchList.Count(); i++)
                        {
                            if(UndiscoveredResearchList.ElementAt(i).Value.Item1 == true)
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
