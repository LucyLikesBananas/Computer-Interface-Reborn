using System;
using ComputerInterface.Enumerations;
using ComputerInterface.Tools;
using UnityEngine;

namespace ComputerInterface.Models.UI;

public class UITextPageHandler : UIPageHandler {
    private string[] _lines;

    public UITextPageHandler(EKeyboardKey prevKey, EKeyboardKey nextKey) : base(prevKey, nextKey) {
    }

    public UITextPageHandler() {
    }

        public void SetText(string text) =>
        SetLines(text.Split('\n'));

        public void SetLines(string[] lines) {
        _lines = lines;
        MaxPage = Mathf.CeilToInt((float)lines.Length / EntriesPerPage) - 1;
        CurrentPage = 0;
        ItemsOnScreen = Math.Min(EntriesPerPage, _lines.Length);
    }

        public string[] GetLinesForPage(int page) {
        if (_lines == null) {
            Logging.Error("Lines are not set yet\nPlease set the lines first");
            return null; 
        }

        var startIdx = EntriesPerPage * page;
        ItemsOnScreen = Math.Min(EntriesPerPage, _lines.Length - startIdx);
        var pageLines = new string[ItemsOnScreen];
        for (var i = 0; i < ItemsOnScreen; i++)
            pageLines[i] = _lines[startIdx + i];

        return pageLines;
    }

        public string[] GetLinesForCurrentPage() =>
        GetLinesForPage(CurrentPage);

        public string GetTextForPage(int page) =>
        string.Join("\n", GetLinesForPage(page));

        public string GetTextForCurrentPage() =>
        GetTextForPage(CurrentPage);
}