using System.Linq;
using System.Reflection;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Tools;

namespace ComputerInterface.Views.GameSettings;

public class CreditsView : ComputerView {
    private int _page;
    private GorillaNetworking.CreditsView _creditsView;

    private int MaxPage => (int)_totalPages.GetValue(_creditsView);
    private PropertyInfo _totalPages;

    private MethodInfo _getPage;

    public override void OnShow(object[] args) {
        if (!BaseGameInterface.CheckForComputer(out var computer)) {
            ShowView<GameSettingsView>();
            return;
        }

        _creditsView = computer.creditsView;
        _creditsView.pageSize = ScreenHeight - 2;
        _totalPages = _creditsView.GetType().GetProperty("TotalPages", BindingFlags.NonPublic | BindingFlags.Instance);
        _getPage = _creditsView.GetType().GetMethod("GetPage", BindingFlags.NonPublic | BindingFlags.Instance);

        
        
        if (_totalPages == null || _getPage == null) {
            Logging.Warning("Computer Interface could not find the expected credits API on this game version.");
            ShowView<GameSettingsView>();
            return;
        }

        base.OnShow(args);
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(GetPage(_page)).Append($"<color=#ffffff50><align=\"center\"><  {_page + 1}/{MaxPage}  ></align></color>");

        SetText(stringBuilder);
    }

    private string GetPage(int page) {
        var text = _getPage.Invoke(_creditsView, [ page ]) as string;
        
        
        if (string.IsNullOrEmpty(text))
            return "";
        var lines = text.Split('\n');
        return string.Join("\n", lines.Take(lines.Length - 2));
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Left:
                _page--;
                if (_page == -1)
                    _page = MaxPage - 1; 
                Redraw();
                break;
            case EKeyboardKey.Right:
                _page++;
                _page %= MaxPage;
                Redraw();
                break;
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
        }
    }
}