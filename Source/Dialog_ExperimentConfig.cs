using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ImmersiveResearch
{
    public class Dialog_ExperimentConfig : Window
    {
        private Building_ExperimentBench _selectedTable;
        private RecipeDef _selectedRecipe;

        private string _selectedExperimentDefName = "";

        LoreWindowDrawingUtility ExpList = new LoreWindowDrawingUtility();
        LoreWindowDrawingUtility ExpTypeList = new LoreWindowDrawingUtility();

        private List<string> _experimentNames = new List<string>();
        private List<string> _experimentTypes = new List<string>();

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(760f, 760f);
            }
        }

        public Dialog_ExperimentConfig()
        {
            InitExperiments();
        }

        public Dialog_ExperimentConfig(Building_ExperimentBench selTable)
        {
            _selectedTable = selTable;
            InitExperiments();
        }

        private Experiment MakeNewExperiment()
        {
            return new Experiment(_selectedRecipe);
        }

        private void InitExperiments()
        {// todo move this to a game component to stop remaking list every window open
            _experimentNames.Add("MechanicalResearch");
            _experimentNames.Add("BiologicalResearch");
            _experimentNames.Add("ConstructionResearch");
            _experimentNames.Add("MetallurgyResearch");
            _experimentNames.Add("WeaponryResearch");
            _experimentNames.Add("ElectricalResearch");
            _experimentNames.Add("MedicalResearch");
            _experimentNames.Add("AdvancedResearch");
            _experimentNames.Add("SpacerResearch");
            _experimentNames.Add("SpacecraftResearch");
            _experimentNames.Add("ModResearch");

            _experimentTypes.Add("Small");
            _experimentTypes.Add("Medium");
            _experimentTypes.Add("Large");
            _experimentTypes.Add("Essential");
            _experimentTypes.Add("Unknown");

            //default selection to dodge nullref exceps
            _selectedRecipe = DefDatabase<RecipeDef>.AllDefsListForReading[0]; 
        }

        public override void DoWindowContents(Rect inRect)
        {// TODO need msgbox on exit to confirm to leave 
            string selectedExp = "";
            string selectedExpType = "";
            string recipeDescription = "Invalid Recipe Combination.";
            
            //title
            Rect rect1 = new Rect(inRect.center.x - 120f, inRect.yMin + 35f, 200f, 74f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Experiment Setup");

            // explain text
            Rect rect2 = new Rect(inRect);
            rect2.yMin = rect1.yMax;
            rect2.yMax -= 38f;
            Text.Font = GameFont.Small;
            Widgets.Label(rect2, "You can perform different types and sizes of experiments here, determining what kind of research you can unlock. The size of the research helps increase your chances of obtaining better research.");

            // 'select experiment' list
            Rect AddExpRect = new Rect(rect2.ContractedBy(70f));
            AddExpRect.width = 550f;
            AddExpRect.height /= 2;
            AddExpRect.y += 20f;
            AddExpRect.x += 275f;

            ExpList.DrawLoreFullList(AddExpRect, _experimentNames, "Experiment Types");
            selectedExp = ExpList.SelectedEntry != null ? ExpList.SelectedEntry.EntryLabel : "None Selected";

            // 'select type' list
            Rect AddExpTypeRect = new Rect(AddExpRect);
            AddExpTypeRect.x = inRect.x;
            AddExpTypeRect.ContractedBy(20f);
            AddExpTypeRect.width = 275f;
            //AddExpTypeRect.y += 20f;

            ExpTypeList.DrawLoreFullList(AddExpTypeRect, _experimentTypes, "Experiment Sizes");
            selectedExpType = ExpTypeList.SelectedEntry != null ? ExpTypeList.SelectedEntry.EntryLabel : "";

            // need to get defName of recipe from this point
            _selectedExperimentDefName = selectedExpType + selectedExp;

            if(!(from t in DefDatabase<RecipeDef>.AllDefsListForReading where t.defName == _selectedExperimentDefName select t).TryRandomElement(out RecipeDef finalDef))
            {
                //Log.Error("Def not found");
            }
            else
            {
                _selectedRecipe = finalDef;
                recipeDescription = _selectedRecipe.description;
            }

            // text explaining selection, e.g. 'Small Biological Research Project - will help unlock biological research'
            Rect rect3 = new Rect(inRect.position, rect2.size);
            rect3.x = inRect.center.x - 150f;
            rect3.y = inRect.yMax - 100f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect3, _selectedExperimentDefName);
            Text.Font = GameFont.Small;
            Rect rect4 = rect3;
            rect4.x = inRect.x;
            rect4.y = inRect.yMax - 250f;
            Widgets.Label(rect4, recipeDescription);

            Rect rect5 = rect4;
            rect5.x = inRect.x;
            rect5.y = inRect.yMax - 180f;
            if (_selectedRecipe.HasModExtension<ResearchDefModExtension>())
            {
                Widgets.Label(rect5, "Potential Research Projects left to discover: " + LoreComputerHarmonyPatches.GetNumOfUnfoundProjsByRecipe(_selectedRecipe));
            }

            // confirm button
            Rect rect6 = new Rect(inRect.center.x - 100f, inRect.yMax - 35f, 150f, 29f);
            if (Widgets.ButtonText(rect6, "Confirm Experiment"))
            {
                if (_selectedRecipe.defName != "ButcherCorpseFlesh")
                {
                    // TODO: change the assumption above to something less naive
                    Bill newExpBill = (Bill_ProductionWithUft)_selectedRecipe.MakeNewBill();
                    Experiment newExp = MakeNewExperiment();
                    _selectedTable.billStack.AddBill(newExpBill);
                    _selectedTable.ExpStack.AddExperiment(newExp);
                     
                    this.Close();
                }
                else
                {
                    Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("No Experiment Selected", delegate
                    {                      
                    }, destructive: true);
                    Find.WindowStack.Add(window);
                }
            }
            // exit button?
        }

        private void CloseMsgBoxWindow(Dialog_MessageBox mb)
        {
            mb.Close();
        }

    }
}
