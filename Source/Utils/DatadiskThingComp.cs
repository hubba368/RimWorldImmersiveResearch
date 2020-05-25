using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    class DatadiskThingComp : ThingComp
    {
        public DatadiskCompProperties Properties => (DatadiskCompProperties)this.Properties;
        public string datadiskDescription;

        public override void CompTick()
        {
            base.CompTick();
        }

    }
}
