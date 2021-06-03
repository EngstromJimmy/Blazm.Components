using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazm.Components.Extensions
{
    public class ObjectExtensions
    {
        public static bool ContainsExt(object obj, string val)
        {
            return obj.ToString()?.IndexOf(val.ToString() ?? "", StringComparison.InvariantCultureIgnoreCase) > -1 ? true : false;
        }   
    }
}
