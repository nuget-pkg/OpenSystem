using Global;
using static Global.EasyObject;

Echo(new { args = args });
Echo(OpenSystem.Add2(11, 22));
int a = 111, b = 222;
var answer = OpenSystem.Add2(a, b);
AssertEqual(expected: 33, actual: answer, hint: new { a, b, answer });