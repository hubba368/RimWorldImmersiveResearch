using RimWorld;
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
        {
            _experimentNames.Add("MechanicalResearch");
            _experimentTypes.Add("Small");
            _selectedRecipe = DefDatabase<RecipeDef>.AllDefsListForReading[0];
        }

        public override void DoWindowContents(Rect inRect)
        {// TODO need msgbox on exit to confirm to leave 
            string selectedExp = "";
            string selectedExpType = "";

            //title
            Rect rect1 = new Rect(inRect).ContractedBy(50f);
            rect1.height = 74f;
            rect1.width = 200f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Experiment Setup");

            // explain text
            Rect rect2 = new Rect(inRect);
            rect2.yMin = rect1.yMax;
            rect2.yMax -= 38f;
            Text.Font = GameFont.Small;
            Widgets.Label(rect2, "You can perform different types and sizes of experiments here, determining what kind of research you can unlock.");

            // 'select experiment' drop down
            Rect AddExpRect = new Rect(rect2);
            AddExpRect.width += 50f;
            AddExpRect.y += 30f;
            
            ExpList.DrawLoreFullList(AddExpRect, _experimentNames);
            selectedExp = ExpList.SelectedEntry != null ? ExpList.SelectedEntry.EntryLabel : "None Selected";

            // 'select type' drop down
            Rect AddExpTypeRect = new Rect(rect2);
            AddExpTypeRect.width += 50f;
            AddExpTypeRect.y += 50f;
            AddExpTypeRect.x += 250f;

            ExpTypeList.DrawLoreFullList(AddExpTypeRect, _experimentTypes);
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
            }

            // text explaining selection, e.g. 'Small Biological Research Project - will help unlock biological research'
            Rect rect3 = rect2;
            rect3.yMin += 110f; 
            Widgets.Label(rect3, _selectedExperimentDefName);
            Rect rect4 = rect2;
            rect4.yMin += 100f;
            Widgets.Label(rect4, _selectedRecipe.description);

            // confirm button
            Rect rect5 = new Rect(rect4.x, rect4.y + 30f, 150f, 29f);
            if (Widgets.ButtonText(rect5, "Confirm Experiment"))
            {
                if (_selectedRecipe.defName != "ButcherCorpseFlesh")
                {
                    // todo message box for confirmation
                    Bill newExpBill = (Bill_ProductionWithUft)_selectedRecipe.MakeNewBill();
                    Experiment newExp = MakeNewExperiment();
                    _selectedTable.billStack.AddBill(newExpBill);
                    _selectedTable.ExpStack.AddExperiment(newExp);
                    if (_selectedRecipe.ProducedThingDef.HasModExtension<ResearchDefModExtension>())
                    {
                        _selectedRecipe.ProducedThingDef.GetModExtension<ResearchDefModExtension>().researchTypes.AddRange(_selectedRecipe.GetModExtension<ResearchDefModExtension>().researchTypes);
                    }
                }
                else
                {
                    // msg box "no experiment has been selected"
                }
            }

            // exit button
        }

    }
}
