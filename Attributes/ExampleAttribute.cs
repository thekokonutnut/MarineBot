using System;
using System.Collections.Generic;
using System.Text;

namespace MarineBot.Attributes
{
    internal class ExampleAttribute : Attribute
    {
        public string[] Example;
        public ExampleAttribute(params string[] example)
        {
            Example = example;
        }
    }
}
