using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Paradigms.Attrs
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PostNotifiablePropertyAttribute : Attribute
    {
    }
}
