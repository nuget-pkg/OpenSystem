// ReSharper disable RedundantUsingDirective
using System.Collections.Generic;
using System.Text;
//using Newtonsoft.Json.Linq;
namespace Misc;
using Global;
//using static Global.EasySystem;
using static Global.EasyObjectClassic;
public class Misc01 {
    public static void Main(string[] args) {
        SetupConsoleEncoding();
        DebugOutput = true;
        Log("⭕️ハロー©⭕️⁅記号⁆◉▶▸⸝↪️ ↩️ ℴ𝓬➺➢ᰔ  ヾ➠✅🈂️❓❗＼／：＊“≪≫￤；‘｀＃％＄＆＾～￤﴾﴿⁅⁆【】≪≫＋ー＊＝⚽ 𝑪𝒉𝒆𝒄𝒌 🌐🪩");
        EasySystem.RunToConsole(Encoding.UTF8, "bash", ["-c", "ls -ltr"]);
        var gvim = EasySystem.FindExeRecursive(@"C:\Program Files\Vim", "gvim.exe");
        Log(gvim, title: "gvim");
    }
}