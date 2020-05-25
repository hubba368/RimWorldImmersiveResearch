using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    [DefOf]
    public static class ResearchHediffDefOf
    {
        public static HediffDef ResearchHediff;

        static ResearchHediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ResearchHediffDefOf));
        }
    }
}
