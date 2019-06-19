using System;
using System.Text;
using UnityEngine;
using Verse;

namespace ImmersiveResearch
{
    public class LoreDrawEntry
    {
        // UNUSED CLASS
        private string _EntryLabel;
        private string _EntryBasicDesc;

        public string EntryLabel
        {
            get
            {
                return _EntryLabel;
            }
        }

        public string EntryBasicDesc
        {
            get
            {
                return _EntryBasicDesc;
            }
        }

        
        public LoreDrawEntry(string label, string basicDescription)
        {
            _EntryLabel = label;
            _EntryBasicDesc = basicDescription;
        }

        public float Draw(float x, float y, float width, bool selected, Action<LoreDrawEntry> clickedCallback, Action<LoreDrawEntry> mousedOverCallback, Vector2 scrollPosition, Rect scrollOutRect)
        {
            float width1 = width * 0.45f;
            Rect rect1 = new Rect(8f, y, width, Verse.Text.CalcHeight("test", width1));
            if ((double)y - (double)scrollPosition.y + (double)rect1.height >= 0.0 && (double)y - (double)scrollPosition.y <= (double)scrollOutRect.height)
            {
                if (selected)
                    Widgets.DrawHighlightSelected(rect1);
                else if (Mouse.IsOver(rect1))
                    Widgets.DrawHighlight(rect1);
                Rect rect2 = rect1;
                rect2.width -= width1;
                Widgets.Label(rect2, _EntryLabel);

                // this 3rd rect is used for entry specific statistics like percentages and in game values.
                /*Rect rect3 = rect1;
                rect3.x = rect2.xMax;
                rect3.width = width1;
                Widgets.Label(rect3, EntryBasicDesc);*/


                if (Widgets.ButtonInvisible(rect1, false))
                {                  
                    clickedCallback(this);
                }
                    
                if (Mouse.IsOver(rect1))
                {
                    
                    mousedOverCallback(this);
                }
                    
            }
            return rect1.height;
        }
    }
}
