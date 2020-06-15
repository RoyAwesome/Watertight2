using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight
{
    public class SubclassOf<T> 
    {
        public static implicit operator SubclassOf<T>(Type operand)
        {
            if (operand == null) return null;
            return new SubclassOf<T>
            { 
                SubclassType = operand
            };

        }

        public static implicit operator Type(SubclassOf<T> operand)
        {
            return operand?.SubclassType;
        }

        public Type SubclassType
        {
            get
            {
                return _subclassType;
            }

            set
            {
                if (value == null) return;
                if(!typeof(T).IsAssignableFrom(value))
                {
                    throw new ArgumentException(value.ToString() + " must inherit from " + (typeof(T)).ToString());
                }
                _subclassType = value;
            }

        }
                

        Type _subclassType;

        public override string ToString()
        {
            return _subclassType?.ToString() ?? "null";
        }
    }
}
