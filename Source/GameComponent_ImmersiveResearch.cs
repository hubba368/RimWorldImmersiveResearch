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
        ResearchDict researchDict;

        public ResearchDict MainResearchDict
        {
            get
            {
                return researchDict;
            }
            set
            {
                researchDict = value;
            }
        }

        public Dictionary<string, int> TempExperimentDict { get => _tempExperimentDict; set => _tempExperimentDict = value; }

        private Dictionary<string, int> _tempExperimentDict = new Dictionary<string, int>();

        public GameComponent_ImmersiveResearch(Game game) : base()
        {
            researchDict = new ResearchDict();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            researchDict.ExposeData();
        }
    }
}
