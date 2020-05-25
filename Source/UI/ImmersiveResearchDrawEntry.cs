using System;
using System.Text;
using UnityEngine;
using Verse;

namespace ImmersiveResearch
{
    public class ImmersiveResearchDrawEntry
    {
        private string _EntryLabel;
        private string _EntryBasicDesc;
        private Texture2D _EntryImage;
        private Thing _EntryAttachedThing;
        private Pawn _EntryAttachedPawn;

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

        public Texture2D EntryImage { get => _EntryImage;}
        public Thing EntryAttachedThing { get => _EntryAttachedThing;}

        public ImmersiveResearchDrawEntry(string label, string basicDescription)
        {
            _EntryLabel = label;
            _EntryBasicDesc = basicDescription;
        }

        public ImmersiveResearchDrawEntry(string label, string basicDesc, Texture2D image)
        {
            _EntryLabel = label;
            _EntryBasicDesc = basicDesc;
            _EntryImage = image;
        }

        public ImmersiveResearchDrawEntry(string label, Thing thing)
        {
            _EntryLabel = label;
            _EntryAttachedThing = thing;
        }

        public float DrawImage(float x, float y, bool selected, float width, Texture2D image, Action<ImmersiveResearchDrawEntry> clickedCallback, Action<ImmersiveResearchDrawEntry> mousedOverCallback, Vector2 scrollPosition, Rect scrollOutRect)
        {
            float width1 = width * 0.45f;
            float height1 = y - 22;
            Rect labelRect = new Rect(x, height1, width, Verse.Text.CalcHeight("test", width1));
            Rect imgRect = new Rect(x, y, image.width / 3, image.height / 3);
            //imgRect.y += labelRect.height;
            //Widgets.ButtonImage(imgRect, image, false); 

            // check for mouse pos and input 
            if ((double)y - (double)scrollPosition.y + (double)imgRect.height >= 0.0 && (double)y - (double)scrollPosition.y <= (double)scrollOutRect.height)
            {
                if (selected)
                    Widgets.DrawHighlightSelected(imgRect);
                else if (Mouse.IsOver(imgRect))
                    Widgets.DrawHighlight(imgRect);
                Rect rect2 = labelRect;
                rect2.width -= width1;
                Widgets.Label(rect2, _EntryBasicDesc);

                if(Widgets.ButtonImage(imgRect, image, true))
                {
                    clickedCallback(this);
                }

                if (Mouse.IsOver(imgRect))
                {
                    mousedOverCallback(this);
                }

            }
           // imgRect.height += labelRect.height;
            return imgRect.height;
        }

        public float Draw(float x, float y, float width, bool selected, Action<ImmersiveResearchDrawEntry> clickedCallback, Action<ImmersiveResearchDrawEntry> mousedOverCallback, Vector2 scrollPosition, Rect scrollOutRect)
        {
            float width1 = width * 0.45f;
            Rect rect1 = new Rect(x, y, width, Verse.Text.CalcHeight("test", width1));
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
