using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UDynamicVars
{
    private Dictionary<string, object> storage = new Dictionary<string, object>();

    public object this[string identifier]
    {
        get
        {
            if (storage.TryGetValue(identifier, out object result))
            {
                return result;
            }
            return null;
        }
        set
        {
            storage[identifier] = value;
        }
    }
}