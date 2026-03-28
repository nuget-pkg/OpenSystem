using Global;
using static Global.EasyObject;

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
        int a = 111, b = 222;
        var answer = OpenSystem.Add2(a, b);
        AssertEqual(expected: 33, actual: answer, hint: new { a, b, answer });
    }
}
