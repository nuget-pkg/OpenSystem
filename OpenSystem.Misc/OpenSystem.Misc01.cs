// ReSharper disable RedundantUsingDirective
using System.Collections.Generic;
//using Newtonsoft.Json.Linq;
namespace Misc;
using Global;
using static Global.EasySystem;
using static Global.EasyObjectClassic;
public class Misc01 {
    public static void Main(string[] args) {
        //Setup
        DebugOutput = true;
        //Log("⭕️ハロー©⭕️");
        Global.EasySystem.RunToConsole("bash", ["-c", "ls -ltr"]);
        var gvim = EasySystem.FindExeRecursive(@"C:\Program Files\Vim", "gvim.exe");
        Log(gvim, title: "gvim");
    }
}