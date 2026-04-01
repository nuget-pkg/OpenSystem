using Global;
using NUnit.Framework;
using static Global.EasyObject;

// ReSharper disable once CheckNamespace
namespace Test;
public class Add2Test {
    [SetUp]
    public void Setup() {
        Echo("Setup() called");
    }
    [Test]
    public void Test01() {
        //AssertIdentical(333, OpenSystem.Add2(111, 222));
    }
}