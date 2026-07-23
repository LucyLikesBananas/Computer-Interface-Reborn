using System;
using ComputerInterface.Enumerations;
using ComputerInterface.Tools;
using UnityEngine;

namespace ComputerInterface.Models.UI;

public class UIElementPageHandler<T> : UIPageHandler {
    private T[] _elements;

    public UIElementPageHandler(EKeyboardKey prevKey, EKeyboardKey nextKey) : base(prevKey, nextKey) {
    }

    public UIElementPageHandler() {
    }

        public void SetElements(T[] elements) {
        _elements = elements;
        MaxPage = Mathf.CeilToInt((float)elements.Length / EntriesPerPage) - 1;
        CurrentPage = 0;
        ItemsOnScreen = Math.Min(EntriesPerPage, _elements.Length);
    }

        public void EnumerateElements(int page, Action<T, int> elementCallback) {
        if (elementCallback == null)
            return;

        var elements = GetElementsForPage(page);
        for (var i = 0; i < elements.Length; i++)
            elementCallback(elements[i], i);
    }

        public void EnumerateElements(Action<T, int> elementCallback) {
        if (elementCallback == null)
            return;

        var elements = GetElementsForPage(CurrentPage);
        for (var i = 0; i < elements.Length; i++)
            elementCallback(elements[i], i);
    }

        public T[] GetElementsForPage(int page) {
        if (_elements == null) {
            Logging.Error("Elements are not set yet\nPlease set the lines first");
            return null;
        }

        var startIdx = EntriesPerPage * page;
        ItemsOnScreen = Math.Min(EntriesPerPage, _elements.Length - startIdx);
        var pageElements = new T[ItemsOnScreen];
        for (var i = 0; i < ItemsOnScreen; i++)
            pageElements[i] = _elements[startIdx + i];

        return pageElements;
    }
}