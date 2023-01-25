using System.Text.RegularExpressions;
using Nananet.Core;

namespace Nananet.App.Nana.Functions.Picture;

public class BukeyiseseCommand : PictureCommand
{
    public override string Name => "bukeyisese";

    private Regex[] _regexes =
    {
        new ("来(点|电|份|张)(涩|瑟|色|美|帅)?图(片|图)? *　*(.*)"),
        new ("來(點|電|份|張)(澀|瑟|色|美|帥)?圖(片|圖)? *　*(.*)"),
    };

    protected override Regex[] Regexes => _regexes;

    private PicCommandHints _hints = new()
    {
        DownloadingHint = "正在检索猫猫数据库，请博士耐心等待...",
        DownloadErrorHint = "图片被猫猫吞噬了，请博士稍后再试。",
        SendErrorHint = "图片被企鹅吞噬了，请博士稍后再试。"
    };

    protected override PicCommandHints Hints => _hints;

    public override async Task Init(IBot bot)
    {
        await base.Init(bot);
        if (PictureRequesters.Count != 0) return;
        PictureRequesters.Add(PictureRequesterStore.WaifuPics);
        PictureRequesters.Add(PictureRequesterStore.NekosBest);
        PictureRequesters.Add(PictureRequesterStore.RandomCatBoy);
    }
    
}