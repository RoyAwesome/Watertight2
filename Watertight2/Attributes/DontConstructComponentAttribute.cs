using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class DontConstructComponentAttribute : Attribute
    {
    }
}
