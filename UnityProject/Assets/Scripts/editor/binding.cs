
using System;
using System.Collections.Generic;
using Puerts;

[Configure]
class ScriptBinding
{
    [Binding]
    static public List<Type> binding
    {
        get
        {
            return new List<Type>() { typeof(Rotate) };
        }
    }
}