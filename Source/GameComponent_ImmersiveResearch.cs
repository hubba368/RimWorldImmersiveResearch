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
