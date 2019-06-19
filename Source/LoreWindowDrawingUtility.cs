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
    /// </summary>
    public class LoreWindowDrawingUtility
    {
        //UNUSED
        private Vector2 _scrollPosition;
        private float _listHeight;
        private List<LoreDrawEntry> _cachedDrawEntries = new List<LoreDrawEntry>();
        private LoreDrawEntry _selectedEntry;
        private LoreDrawEntry _mousedOverEntry;
        

        public LoreDrawEntry SelectedEntry
        {
            get
            {
                return _selectedEntry;
            }
        }

        // fill up cachedEntries list with our Things
        public void DrawLoreFullList(Rect inRect, List<string> thingList)
        {
            if (_cachedDrawEntries.NullOrEmpty<LoreDrawEntry>())
            {
                foreach(string pawn in thingList)
                {
                    string label = pawn;
                    string desc = "";
                    LoreDrawEntry newEntry = new LoreDrawEntry(label, desc);
                    _cachedDrawEntries.Add(newEntry);
                }
            }
            DrawLoreListWorker(inRect);
        }

        // Draw our list to the UI
        // mostly just ripped from original source code, jsut changed in a few areas for my needs
        private void DrawLoreListWorker(Rect refRect)
        {
            Rect rect1 = new Rect(refRect);
            rect1.width *= 0.5f;
            Rect rect2 = new Rect(refRect);
            rect2.x = rect1.xMax;
            rect2.width = refRect.xMax - rect2.x;
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(0.0f, 0.0f, rect1.width - 16f, _listHeight);
            Widgets.BeginScrollView(rect1, ref _scrollPosition, viewRect, true);
            float curY = 0.0f;
            string entryName = (string)null;
            _mousedOverEntry = (LoreDrawEntry)null;

            for(int index = 0; index < _cachedDrawEntries.Count; ++index)
            {
                Action<LoreDrawEntry> mouseClickEvent = MouseClickCallBackEvent;
                Action<LoreDrawEntry> mouseOverEvent = MouseOverCallBackEvent;

                curY += _cachedDrawEntries[index].Draw(8f, curY, viewRect.width - 8f,
                   false, mouseClickEvent, mouseOverEvent, _scrollPosition, rect1);
            }

            _listHeight = curY + 100f;
            Widgets.EndScrollView();
            Rect rect3 = rect2.ContractedBy(10f);
            GUI.BeginGroup(rect3);
            LoreDrawEntry loreEntry = _selectedEntry ?? _mousedOverEntry ?? _cachedDrawEntries.FirstOrDefault<LoreDrawEntry>();

            if (loreEntry != null)
            {
                // draw descriptions etc (text on the right side of the OG inspection window)
                Widgets.Label(rect3.AtZero(), loreEntry.EntryBasicDesc);
            }
            GUI.EndGroup();
        }


        private void SelectEntry(LoreDrawEntry entry, bool playSound = true)
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
        private void MouseClickCallBackEvent(LoreDrawEntry r)
        {
            SelectEntry(r, true);
        }

        private void MouseOverCallBackEvent(LoreDrawEntry r)
        {
            _mousedOverEntry = r;
        }
    }
}
