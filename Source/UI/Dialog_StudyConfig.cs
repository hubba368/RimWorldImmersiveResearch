using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ImmersiveResearch
{
    public class Dialog_StudyConfig : Window
    {
        private Building_StudyTable _selectedTable;

        private ImmersiveResearchWindowDrawingUtility _expsInColony = new ImmersiveResearchWindowDrawingUtility();
        private ImmersiveResearchWindowDrawingUtility _colonyResearchers = new ImmersiveResearchWindowDrawingUtility();

        private List<Tuple<string, Thing>> _finishedExperimentList = new List<Tuple<string, Thing>>();
        private List<Tuple<string, Thing>> _pawnsInColony = new List<Tuple<string, Thing>>();

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(760f, 760f);
            }
        }

        public Dialog_StudyConfig(Building_StudyTable table)
        {
            _selectedTable = table;

            var pawns = Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);

            for(int i = 0; i < pawns.Count; ++i)
            {
                if (pawns[i].IsColonist)
                {
                    // remove non research assigned pawns
                    if (pawns[i].workSettings.WorkIsActive(WorkTypeDefOf.Research) && !pawns[i].WorkTypeIsDisabled(WorkTypeDefOf.Research))
                    {
                        _pawnsInColony.Add(new Tuple<string, Thing>(pawns[i].Name.ToString(), pawns[i]));
                    }
                }
            }



            if(!(from t in DefDatabase<ThingDef>.AllDefs where t.defName == "FinishedExperiment" select t).TryRandomElement(out ThingDef finalDef))
            {
                Log.Error("Unable to locate finished experiment def in DefDatabase.", false);
            }
            else
            {
                var def = finalDef;
                var req = ThingRequest.ForDef(def);
                var thingList = Find.CurrentMap.listerThings.ThingsMatching(req);

                if(thingList.Count == 0)
                {
                   // Log.Error("No finished exps in colony.");
                    return;
                }

                foreach(var thing in thingList)
                {
                    var comp = thing.TryGetComp<ResearchThingComp>();

                    _finishedExperimentList.Add(new Tuple<string, Thing>(thing.TryGetComp<ResearchThingComp>().researchDefName, thing));
                }
               // Log.Error("Num of experiments in colony: " + thingList.Count);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Thing selectedExperiment = null;
            string selectedExperimentName = "";
            Pawn selectedPawn = null;
            bool isExperimentSelected = false;

            RecipeDef selectedRecipeDef = null;
            selectedRecipeDef = DefDatabase<RecipeDef>.AllDefsListForReading[0];

            //title
            Rect rect1 = new Rect(inRect.center.x - 70f, inRect.yMin + 35f, 200f, 74f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Study Setup");

            // Exit button
            Rect exitRect = new Rect(inRect.xMax - 50f, inRect.yMin + 5f, 50f, 30f);
            if (Widgets.ButtonText(exitRect, "Exit"))
            {
                this.Close();
            }

            // explain text
            Rect rect2 = new Rect(inRect);
            rect2.yMin = rect1.yMax;
            rect2.yMax -= 38f;
            Text.Font = GameFont.Small;
            Widgets.Label(rect2, "Here you can choose an experiment for a colonist to study. Studying experiments will enable your colony to better retain its scientific knowledge. Any colonist can study a research project, however those with lower intellectual skills will take a much longer time to complete their studies.");

            // 'select experiment' list
            Rect AddExpRect = new Rect(rect2);
            AddExpRect.width = 550f;//275f;
            AddExpRect.height /= 2;
            AddExpRect.y += 70f;
            AddExpRect.x += 370f;


            if (_finishedExperimentList.Count == 0)
            {
                var list = new List<string>();
                list.Add("No experiments");
                _expsInColony.DrawTextList(AddExpRect, list, "No experiments in Colony");
                isExperimentSelected = false;
            }
            else
            {
                selectedExperiment = _expsInColony.SelectedEntry != null ? _expsInColony.SelectedEntry.EntryAttachedThing : null;
                selectedExperimentName = _expsInColony.SelectedEntry?.EntryLabel;
                _expsInColony.DrawTextListWithAttachedThing(AddExpRect, _finishedExperimentList, "All Experiments In Colony");
            }

            // 'select researcher' list
            Rect AddPawnRect = new Rect(AddExpRect);
            AddPawnRect.x -= 310f;
            //AddPawnRect.ContractedBy(30f);
            AddPawnRect.width = 550f;//275f;

            if (_pawnsInColony.Count == 0)
            {
                var list = new List<string>();
                list.Add("No researchers");
                _expsInColony.DrawTextList(AddExpRect, list, "No researchers in Colony");
                isExperimentSelected = false;
            }
            else
            {
                selectedPawn = (Pawn)_colonyResearchers.SelectedEntry?.EntryAttachedThing;
                _colonyResearchers.DrawTextListWithAttachedThing(AddPawnRect, _pawnsInColony, "Researchers In Colony");
            }


            // need to get defName of recipe from this point
            string _selectedOption = selectedPawn?.Name.ToString() + " will study: " + selectedExperimentName;


            string warningText = "";
            if (selectedPawn != null)
            {
                warningText = selectedPawn.Name.ToString() + " is a colony researcher with a skill of: " + selectedPawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt;
            }
            
            // text explaining selected pawn and topic
            Rect rect3 = new Rect(inRect.position, rect2.size);
            rect3.x = inRect.center.x - 300f;
            rect3.y = inRect.yMax - 100f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect3, _selectedOption);

            // optional warning text for pawns with no research / already studied topic
            Text.Font = GameFont.Small;
            Rect rect4 = rect3;
            rect4.x = inRect.x;
            rect4.y = inRect.yMax - 210f;
            Widgets.Label(rect4, warningText);

            // get recipe def
            if (!(from t in DefDatabase<RecipeDef>.AllDefsListForReading where t.defName == "StudyFinishedExperiment" select t).TryRandomElement(out RecipeDef finalDef))
            {
                //Log.Error("Def not found");
            }
            else
            {
                selectedRecipeDef = finalDef;
            }

            if(selectedPawn != null && selectedExperimentName != "")
            {
                isExperimentSelected = true;
            }

            Rect rect5 = rect4;
            rect5.x = inRect.x;
            rect5.y = inRect.yMax - 180f;

            // confirm button
            Rect rect6 = new Rect(inRect.center.x - 80f, inRect.yMax - 35f, 150f, 29f);
            if (Widgets.ButtonText(rect6, "Confirm Study"))
            {
                if (isExperimentSelected == true)
                {
                    Experiment newStudy = new Experiment(selectedRecipeDef);
                    Bill newBill = (Bill_Production)selectedRecipeDef.MakeNewBill();

                    newStudy.uniquePawnDoer = selectedPawn;
                    newStudy.uniqueThingIng = selectedExperiment;
                    _selectedTable.ExpStack.AddExperiment(newStudy);
                    _selectedTable.ExpStack.AddExperimentWithBill(newBill);
                    newBill.pawnRestriction = selectedPawn;
                    selectedExperiment.def.GetModExtension<ResearchDefModExtension>().ResearchDefAttachedToExperiment = selectedExperimentName;
                    isExperimentSelected = false;
                    this.Close();
                }
                else
                {
                    Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("No Study Option Selected", delegate
                    {
                    }, destructive: true);
                    Find.WindowStack.Add(window);
                }
            }
        }
    }
}
