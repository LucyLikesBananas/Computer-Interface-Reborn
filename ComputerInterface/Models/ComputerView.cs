using ComputerInterface.Interfaces;
using ComputerInterface.Views;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ComputerInterface.Enumerations;
using UnityEngine;

namespace ComputerInterface.Models;

public class ComputerView : IComputerView {
        public static int ScreenWidth = 52;

        public static int ScreenHeight = 12;

    public string PrimaryColor = "ed6540";

        public string Text {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    protected string _text;

    public Type CallerViewType { get; set; }

        public virtual void SetText(StringBuilder stringBuilder) =>
        Text = stringBuilder.ToString();

        public virtual void SetText(Action<StringBuilder> builderCallback) {
        StringBuilder stringBuilder = new();
        builderCallback(stringBuilder);
        SetText(stringBuilder);
    }

        public virtual void OnKeyPressed(EKeyboardKey key) {
    }

        public virtual void OnShow(object[] args) =>
        RaisePropertyChanged(nameof(Text));

        public void ShowView<T>(params object[] args) =>
        ShowView(typeof(T), args);

        public void ShowView(Type type, params object[] args) =>
        OnViewSwitchRequest?.Invoke(new ComputerViewSwitchEventArgs(GetType(), type, args));

        public void ReturnView() {
        
        
        if (CallerViewType == null) {
            ReturnToMainMenu();
            return;
        }
        ShowView(CallerViewType);
    }

        public void ReturnToMainMenu() =>
        ShowView<MainMenuView>();

    public void SetBackground(Texture texture, Color? color = null) {
        ComputerViewChangeBackgroundEventArgs args = new(texture, color);
        OnChangeBackgroundRequest?.Invoke(args);
    }

    public void RevertBackground() =>
        OnChangeBackgroundRequest?.Invoke(null);

    public async Task ShowSplashForDuration(Texture texture, int milliseconds) {
        var text = Text;
        Text = "";
        SetBackground(texture);
        await Task.Delay(milliseconds);
        RevertBackground();
        Text = text;
    }

    public event ComputerViewSwitchEventArgs.ComputerViewSwitchEventHandler OnViewSwitchRequest;
    public event ComputerViewChangeBackgroundEventArgs.ComputerViewChangeBackgroundEventHandler OnChangeBackgroundRequest;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}