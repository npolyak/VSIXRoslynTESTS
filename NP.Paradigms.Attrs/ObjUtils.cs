using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Paradigms.Attrs
{
    public static class ObjUtils
    {
        public static bool ObjEquals(this object obj1, object obj2)
        {
            if (obj1 == null)
                return obj2 == null;

            return obj1.Equals(obj2);
        }
    }
}
