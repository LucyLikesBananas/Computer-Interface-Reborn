using System.Text;
using ComputerInterface.Enumerations;
using UnityEngine;

namespace ComputerInterface.Models.UI;

public class UIPageHandler {
    public int CurrentPage { get; set; }

        public int MaxPage { get; protected set; }

        public int EntriesPerPage { get; set; }

        public int ItemsOnScreen { get; protected set; }

        public string Footer = "{0} {2}/{3} {1}";

    public string PrevMark = "<";
    public string NextMark = ">";

    private readonly bool _useKeys;
    private readonly EKeyboardKey _prevKey;
    private readonly EKeyboardKey _nextKey;


    public UIPageHandler(EKeyboardKey prevKey, EKeyboardKey nextKey) {
        _prevKey = prevKey;
        _nextKey = nextKey;
        _useKeys = true;
    }

    public UIPageHandler() {
    }

    public bool HandleKeyPress(EKeyboardKey key) {
        if (!_useKeys)
            return false;

        if (key == _prevKey) {
            PreviousPage();
            return true;
        }

        if (key == _nextKey) {
            NextPage();
            return true;
        }

        return false;
    }

        public void NextPage() {
        if (CurrentPage < MaxPage)
            CurrentPage++;
    }

        public void PreviousPage() {
        if (CurrentPage > 0)
            CurrentPage--;
    }

        public int MovePageToIdx(int idx) {
        var page = Mathf.FloorToInt((float)idx / EntriesPerPage);
        CurrentPage = page;
        return idx % EntriesPerPage;
    }

        public int GetAbsoluteIndex(int page, int itemIdx) =>
        page * EntriesPerPage + itemIdx;

        public int GetAbsoluteIndex(int itemIdx) =>
        GetAbsoluteIndex(CurrentPage, itemIdx);

    public void AppendFooter(StringBuilder str) {
        for (var i = 0; i < EntriesPerPage - ItemsOnScreen; i++)
            str.AppendLine();

        str.Append(GetFooter());
    }

    private string GetFooter() =>
        string.Format(Footer, CurrentPage > 0 ? PrevMark : " ", CurrentPage < MaxPage ? NextMark : " ", CurrentPage + 1, MaxPage + 1);
}