// ReSharper disable RedundantUsingDirective
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
namespace Misc;
using Global;
using static Global.EasyObject;
using static Global.EasySystem;
public class Misc01 {
    public static void Main(string[] args) {
        SetupConsoleEncoding();
        ShowLineNumbers = false;
        ShowDetail = true;
        UseAnsiConsole = true;
        DebugOutput = true;
        Log("⭕️ハロー©⭕️");
        EasySystem.RunToConsole("bash", ["-c", "ls -ltr"]);
        var exe = EasySystem.FindExePath("Notepad++.exe");
        Log(exe);
        if (exe != null)
            EasySystem.RunToConsole(exe, [@"C:\home17\+sub\nuget.org\OpenSystem\build", "-n3"]);
    }
}