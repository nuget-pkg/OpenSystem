using System;
using Global;
using static Global.EasyObjectClassic;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Demo;

// ReSharper disable once ArrangeTypeModifiers
static class Program
{
    // ReSharper disable once ArrangeTypeMemberModifiers
    static void Main(string[] args)
    {
        Echo(new { args = args });
        Echo(OpenSystem.Add2(11, 22));
   }
}
