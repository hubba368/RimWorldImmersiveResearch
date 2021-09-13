using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ImmersiveResearch
{
    public class ResearchDefModExtension :DefModExtension
    {
        public List<ResearchTypes> researchTypes = new List<ResearchTypes>();

        public ResearchSizes ResearchSize;

        public string ResearchDefAttachedToExperiment;

        public bool ExperimentHasBeenMade;

        public string modResearchType = "";
    }
}
