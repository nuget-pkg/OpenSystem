// ReSharper disable RedundantUsingDirective

//using Spectre.Console.Json.Syntax;

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Misc;

using Global;
using static Global.EasyObject;
using static Global.EasySystem;

public class Misc01
{
    public static void Main(string[] args)
    {
        SetupConsoleEncoding();
        ShowLineNumbers = false;
        ShowDetail = true;
        UseAnsiConsole = true;
        DebugOutput = true;
        Log("⭕️ハロー©⭕️");
        EasySystem.RunToConsole("bash", ["-c", "ls -ltr"]);
        JArray? newton = EasyToolbox.FromJson("[11,null,33.15,[44,55], {'a': 123}]");
        Log(newton, title: "newton");
        if (newton != null)
        {
            var o = newton.ToObject<object[]>();
            Log(o);
        }

        var o2 = EasyToolbox.FromObject(newton);
        Log(o2, title: "o2");
        var eo = ParseObject(newton);
        Log(eo, title: "eo");
    }

    public static EasyObject ParseObject(dynamic? x)
    {
        if (x == null) return Null;
        if (x is JArray jarray)
        {
            //return FromObject(array.ToObject<object[]>());
            List<object> array = jarray.ToObject<List<object>>()!;
            var result = NewArray();
            foreach (var item in array)
            {
                result.Add(ParseObject(item));
            }

            return result;
        }
        else if (x is JObject jobject)
        {
            Dictionary<string, object> dict = jobject.ToObject<Dictionary<string, object>>()!;
            var result = NewObject();
            var keys = dict.Keys;
            //return FromObject(keys);
            foreach (var key in keys)
            {
                result.Add(key, ParseObject(dict[key]));
            }

            return result;
        }
        else
        {
            string typeName = FullName(x);
            if (typeName.StartsWith("System."))
            {
                return FromObject(x);
            }

            return FullName(x);
        }
    }
}