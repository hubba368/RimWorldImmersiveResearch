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
    public struct ExperimentConfigUIElement
    {
        public Texture2D UIIcon { get; }
        public string PartialDefName { get; }
        public string UIText { get; }

        public ExperimentConfigUIElement(Texture2D icon, string def, string text)
        {
            UIIcon = icon;
            PartialDefName = def;
            UIText = text;
        }
    }

    public class Dialog_ExperimentConfig : Window
    {
        private Building_ExperimentBench _selectedTable;
        private RecipeDef _selectedRecipe;

        private string _selectedExperimentDefName = "";

        ImmersiveResearchWindowDrawingUtility ExpList = new ImmersiveResearchWindowDrawingUtility();
        ImmersiveResearchWindowDrawingUtility ExpTypeList = new ImmersiveResearchWindowDrawingUtility();

        private List<ExperimentConfigUIElement> _experimentTypes = new List<ExperimentConfigUIElement>();
        private List<ExperimentConfigUIElement> _experimentNames = new List<ExperimentConfigUIElement>();


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

            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.MechanicalIcon, "MechanicalResearch", "Mechanical"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.BiologicalIcon, "BiologicalResearch", "Biological"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.TextilesIcon, "TextilesResearch", "Textiles"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.CulturalIcon, "CulturalResearch", "Cultural"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ConstructionIcon, "ConstructionResearch", "Construction"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.MetallurgyIcon, "MetallurgyResearch", "Metallurgy"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.WeaponryIcon, "WeaponryResearch", "Weaponry"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ElectricalIcon, "ElectricalResearch", "Electrical"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.MedicalIcon, "MedicalResearch", "Medical"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.AdvancedIcon, "AdvancedResearch", "Advanced"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.SpacerIcon, "SpacerResearch", "Spacer"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.UltratechIcon, "UltratechResearch", "Ultratech"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.SpacecraftIcon, "SpacecraftResearch", "Spacecraft"));
            _experimentNames.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ModIcon, "ModResearch", "Mod"));

            _experimentTypes.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ResearchSizeSmallIcon, "Small", "Small"));
            _experimentTypes.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ResearchSizeMediumIcon, "Medium", "Medium"));
            _experimentTypes.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ResearchSizeLargeIcon, "Large", "Large"));
            _experimentTypes.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ResearchSizeEssentialIcon, "Essential", "Essential"));
            _experimentTypes.Add(new ExperimentConfigUIElement(ImmersiveResearchTextures.ResearchSizeModIcon, "Unknown", "Unknown"));

            //default selection to dodge nullref exceps
            _selectedRecipe = DefDatabase<RecipeDef>.AllDefsListForReading[0];

            ExpList.InitImageList(_experimentNames);
            ExpTypeList.InitImageList(_experimentTypes);
        }

        public override void DoWindowContents(Rect inRect)
        {
            string selectedExp = "";
            string selectedExpType = "";
            string recipeDescription = "Invalid Recipe Combination.";
            bool isExperimentSelected = false;
            
            //title
            Rect rect1 = new Rect(inRect.center.x - 80f, inRect.yMin + 35f, 200f, 74f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Experiment Setup");

            // Exit button
            Rect exitRect = new Rect(inRect.xMax - 50f, inRect.yMin + 5f, 50f, 30f);
            if(Widgets.ButtonText(exitRect, "Exit"))
            {
                this.Close();
            }

            // explain text
            Rect rect2 = new Rect(inRect);
            rect2.yMin = rect1.yMax;
            rect2.yMax -= 38f;
            Text.Font = GameFont.Small;
            Widgets.Label(rect2, "You can perform different types and sizes of experiments here, determining what kind of research you can unlock. The size of the research helps increase your chances of obtaining better research.");

            // 'select experiment' list
            Rect AddExpRect = new Rect(rect2);
            AddExpRect.width = 550f;//275f;
            AddExpRect.height /= 2;
            AddExpRect.y += 70f;
            AddExpRect.x += 370f;

            //ExpList.DrawTextList(AddExpRect, _experimentNames, "Experiment Types");
            selectedExp = ExpList.SelectedEntry != null ? ExpList.SelectedEntry.EntryLabel : "None Selected";
            ExpList.DrawImageList(AddExpRect, "Experiment Types");

            // 'select type' list
            Rect AddExpTypeRect = new Rect(AddExpRect);
            AddExpTypeRect.x -= 310f;
            //AddExpTypeRect.ContractedBy(20f);
            AddExpTypeRect.width = 550f;//275f;
            //AddExpTypeRect.y += 20f;

            //ExpTypeList.DrawTextList(AddExpTypeRect, _experimentTypes, "Experiment Sizes");
            selectedExpType = ExpTypeList.SelectedEntry != null ? ExpTypeList.SelectedEntry.EntryLabel : "";
            ExpTypeList.DrawImageList(AddExpTypeRect, "Experiment Sizes");

            // need to get defName of recipe from this point
            _selectedExperimentDefName = selectedExpType + selectedExp;

            if(!(from t in DefDatabase<RecipeDef>.AllDefsListForReading where t.defName == _selectedExperimentDefName select t).TryRandomElement(out RecipeDef finalDef))
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

            // text explaining selection, e.g. 'Small Biological Research Project - will help unlock biological research'
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
                    // TODO: change the assumption above to something less naive
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
            // exit button?
        }

        private void CloseMsgBoxWindow(Dialog_MessageBox mb)
        {
            mb.Close();
        }

    }
}
