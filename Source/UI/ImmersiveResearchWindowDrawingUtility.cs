using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ImmersiveResearch
{
    /// <summary>
    ///  Custom implementation of RimWorld class 'StatsReportUtility'.
    ///  We are using the previously mentioned class' functionality to allow the player to click on list entries.
    /// </summary>
    public class ImmersiveResearchWindowDrawingUtility
    {

        private Vector2 _scrollPosition;
        private float _listHeight;
        private List<ImmersiveResearchDrawEntry> _cachedDrawEntries = new List<ImmersiveResearchDrawEntry>();
        private ImmersiveResearchDrawEntry _selectedEntry;
        private ImmersiveResearchDrawEntry _mousedOverEntry;
        List<Rect> testRectList = new List<Rect>();      

        public ImmersiveResearchDrawEntry SelectedEntry
        {
            get
            {
                return _selectedEntry;
            }
        }

        public void InitImageList(List<ExperimentConfigUIElement> images)
        {
            // setup table layout
            float rowHeight = images[0].UIIcon.height / 3;
            float columnWidth = images[0].UIIcon.width / 3;
            float curY = 22.0f; // offset for text labels 

            for (int index = 0; index < images.Count; index+=2)
            {
                var r1 = new Rect(15f, curY, rowHeight, columnWidth);
                testRectList.Add(r1);
                var r2 = new Rect(r1.xMax + 35f, curY, rowHeight, columnWidth);
                testRectList.Add(r2);

                curY += r1.height + 22f;
            }

            if (_cachedDrawEntries.NullOrEmpty())
            {
                for(int i = 0; i < images.Count; ++i)
                {
                    ImmersiveResearchDrawEntry newEntry = new ImmersiveResearchDrawEntry(images[i].PartialDefName, images[i].UIText, images[i].UIIcon);
                    _cachedDrawEntries.Add(newEntry);
                }
            }
        }

        public void DrawTextList(Rect inRect, List<string> thingList, string listTitle)
        {
            if (_cachedDrawEntries.NullOrEmpty())
            {
                foreach(string str in thingList)
                {
                    string label = str;
                    string desc = "";
                    ImmersiveResearchDrawEntry newEntry = new ImmersiveResearchDrawEntry(label, desc);
                    _cachedDrawEntries.Add(newEntry);
                }
            }
            DrawListWorker(inRect, listTitle, false);
        }

        public void DrawTextListWithAttachedThing(Rect inRect, List<Tuple<string, Thing>> thingList, string listTitle)
        {
            if (_cachedDrawEntries.NullOrEmpty())
            {
                foreach (var str in thingList)
                {
                    string label = str.Item1;
                    ImmersiveResearchDrawEntry newEntry = new ImmersiveResearchDrawEntry(label, str.Item2);
                    _cachedDrawEntries.Add(newEntry);
                }
            }
            DrawListWorker(inRect, listTitle, false);
        }

        public void DrawImageList(Rect inRect, string listTitle)
        {
            DrawListWorker(inRect, listTitle, true);
        }


        // Draw our list to the UI
        // mostly just ripped from original source code, jsut changed in a few areas for my needs
        private void DrawListWorker(Rect refRect, string listTitle, bool hasImage)
        {
            Rect titleRect = new Rect(refRect);
            titleRect.x = refRect.xMin + 50f;
            titleRect.y = refRect.yMin;
            Widgets.Label(titleRect, listTitle);

            Rect rect1 = new Rect(refRect);
            rect1.width *= 0.5f;
            rect1.y += 25f;
            Rect rect2 = new Rect(refRect);
            rect2.x = rect1.xMax;
            rect2.width = refRect.xMax - rect2.x;
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(0.0f, 0.0f, rect1.width - 16f, _listHeight);
            Widgets.DrawMenuSection(rect1);
            Widgets.BeginScrollView(rect1, ref _scrollPosition, viewRect, true);
            float curY = 0.0f;
  
            _mousedOverEntry = (ImmersiveResearchDrawEntry)null;

            if (hasImage)
            {
                for(int i = 0; i < _cachedDrawEntries.Count; ++i)
                {
                    Action<ImmersiveResearchDrawEntry> mouseClickEvent = MouseClickCallBackEvent;
                    Action<ImmersiveResearchDrawEntry> mouseOverEvent = MouseOverCallBackEvent;
                    curY += _cachedDrawEntries[i].DrawImage(testRectList[i].x, testRectList[i].y, false, viewRect.width - 8f, _cachedDrawEntries[i].EntryImage, mouseClickEvent, mouseOverEvent, _scrollPosition, rect1);
                }
            }
            else
            {
                for (int i = 0; i < _cachedDrawEntries.Count; ++i)
                {
                    Action<ImmersiveResearchDrawEntry> mouseClickEvent = MouseClickCallBackEvent;
                    Action<ImmersiveResearchDrawEntry> mouseOverEvent = MouseOverCallBackEvent;

                    curY += _cachedDrawEntries[i].Draw(8f, curY, viewRect.width - 8f,
                        false, mouseClickEvent, mouseOverEvent, _scrollPosition, rect1);
                }
            }


            _listHeight = curY;
            Widgets.EndScrollView();
           // Rect rect3 = rect2.ContractedBy(10f);
            //GUI.BeginGroup(rect3);
            ImmersiveResearchDrawEntry loreEntry = _selectedEntry ?? _mousedOverEntry ?? _cachedDrawEntries.FirstOrDefault<ImmersiveResearchDrawEntry>();

            if (loreEntry != null)
            {
                // draws text on the right side 
                //Widgets.Label(rect3.AtZero(), loreEntry.EntryBasicDesc);
            }
            //GUI.EndGroup();
        }


        private void SelectEntry(ImmersiveResearchDrawEntry entry, bool playSound = true)
        {
            _selectedEntry = entry;
            if (!playSound)
            {
                return;
            }
            else
            {
                return;
            }
        }


        // Mouse events 
        private void MouseClickCallBackEvent(ImmersiveResearchDrawEntry r)
        {
            SelectEntry(r, true);
        }

        private void MouseOverCallBackEvent(ImmersiveResearchDrawEntry r)
        {
            _mousedOverEntry = r;
        }
    }
}
