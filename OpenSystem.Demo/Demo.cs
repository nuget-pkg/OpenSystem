using Global;
using static Global.EasyObject;
try {
    Echo(new { args = args });
    //Echo(OpenSystem.Add2(11, 22));
    int a = 111, b = 222;
    var answer = a + b; //OpenSystem.Add2(a, b);
    AssertEquivalent(expected: 33, actual: answer, hint: new { a, b, answer });
}
catch (System.Exception ex) {
    //Abort(ex);
    HandleBetrayedExpectation(ex,
        NewObject(
            "abc", 123,
            "xyz",
            NewArray(11, 22, 33)
        ),
        7777);
}