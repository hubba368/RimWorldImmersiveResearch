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
    public class Dialog_ModExperimentConfig : Window
    {
        private Building_ExperimentBench _selectedTable;
        private RecipeDef _selectedRecipe;

        private string _selectedExperimentDefName = "";

        private List<string> _modDefNames = new List<string>();
        private List<ExperimentConfigUIElement> _experimentTypes = new List<ExperimentConfigUIElement>();

        ImmersiveResearchWindowDrawingUtility _modExpList = new ImmersiveResearchWindowDrawingUtility();
        ImmersiveResearchWindowDrawingUtility _modExpTypeList = new ImmersiveResearchWindowDrawingUtility();

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(760f, 760f);
            }
        }

        public Dialog_ModExperimentConfig()
        {
            InitModExperiments();
        }

        public Dialog_ModExperimentConfig(Building_ExperimentBench selTable)
        {
            _selectedTable = selTable;
            InitModExperiments();
        }

        private void InitModExperiments()
        {
            _modDefNames = LoreComputerHarmonyPatches.ModResearchDefNameList;

            _experimentTypes.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ResearchSizeSmallIcon, "Small", "Small"));
            _experimentTypes.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ResearchSizeMediumIcon, "Medium", "Medium"));
            _experimentTypes.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ResearchSizeLargeIcon, "Large", "Large"));

            _selectedRecipe = DefDatabase<RecipeDef>.AllDefsListForReading[0];
            _modExpTypeList.InitImageList(_experimentTypes);
        }

        private Experiment MakeNewExperiment()
        {
            return new Experiment(_selectedRecipe);
        }

        public override void DoWindowContents(Rect inRect)
        {
            string selectedExp = "";
            string selectedExpType = "";
            string recipeDescription = "Invalid Recipe Combination.";
            bool isExperimentSelected = false;

            //title
            Rect rect1 = new Rect(inRect.center.x - 80f, inRect.yMin + 35f, 250f, 74f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Mod Experiment Setup");

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
            Widgets.Label(rect2, "Here you can perform experiments for any mods you have installed. Mod research projects have been automatically grouped with their respective mod names and sizes, based on their cost.");

            // 'select experiment' list
            Rect AddExpRect = new Rect(rect2);
            AddExpRect.width = 550f;//275f;
            AddExpRect.height /= 2;
            AddExpRect.y += 70f;
            AddExpRect.x += 370f;

            _modExpList.DrawTextList(AddExpRect, _modDefNames, "Experiment Categories");
            selectedExp = _modExpList.SelectedEntry != null ? _modExpList.SelectedEntry.EntryLabel : "None Selected";

            // 'select type' list
            Rect AddExpTypeRect = new Rect(AddExpRect);
            AddExpTypeRect.x -= 310f;
            //AddExpTypeRect.ContractedBy(20f);
            AddExpTypeRect.width = 550f;//275f;
            //AddExpTypeRect.y += 20f;

            //ExpTypeList.DrawTextList(AddExpTypeRect, _experimentTypes, "Experiment Sizes");
            selectedExpType = _modExpTypeList.SelectedEntry != null ? _modExpTypeList.SelectedEntry.EntryLabel : "";
            _modExpTypeList.DrawImageList(AddExpTypeRect, "Experiment Sizes");

            // need to get defName of recipe from this point
            _selectedExperimentDefName = selectedExpType + selectedExp;

            if (!(from t in DefDatabase<RecipeDef>.AllDefsListForReading where t.defName == _selectedExperimentDefName select t).TryRandomElement(out RecipeDef finalDef))
            {
                //Log.Error("Def not found");
                isExperimentSelected = false;
            }
            else
            {
                isExperimentSelected = true;
                _selectedRecipe = finalDef;
                recipeDescription = _selectedRecipe.description;
            }

            // text explaining selection
            Rect rect3 = new Rect(inRect.position, rect2.size);
            rect3.x = inRect.center.x - 150f;
            rect3.y = inRect.yMax - 100f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect3, _selectedExperimentDefName);

            Text.Font = GameFont.Small;
            Rect rect4 = rect3;
            rect4.x = inRect.x;
            rect4.y = inRect.yMax - 210f;
            Widgets.Label(rect4, recipeDescription);

            Rect rect5 = rect4;
            rect5.x = inRect.x;
            rect5.y = inRect.yMax - 160f;
            if (_selectedRecipe.HasModExtension<ResearchDefModExtension>())
            {
                Widgets.Label(rect5, "Potential Research Projects left to discover: " + LoreComputerHarmonyPatches.GetNumOfUnfoundProjsByRecipe(_selectedRecipe));              
            }

            // confirm button
            Rect rect6 = new Rect(inRect.center.x - 100f, inRect.yMax - 35f, 150f, 29f);
            if (Widgets.ButtonText(rect6, "Confirm Experiment"))
            {

                if (isExperimentSelected == true)
                {
                    Bill newExpBill = (Bill_ProductionWithUft)_selectedRecipe.MakeNewBill();
                    Experiment newExp = MakeNewExperiment();
                    _selectedTable.ExpStack.AddExperiment(newExp);
                    _selectedTable.ExpStack.AddExperimentWithBill(newExpBill);
                    isExperimentSelected = false;
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
        }
    }
}
