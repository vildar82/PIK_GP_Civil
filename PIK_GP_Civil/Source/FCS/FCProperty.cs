using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Gis.Map.Classification;

namespace PIK_GP_Civil.FCS
{
    public class FCProperty
    {        
        public string Name { get; }        
        public object Value { get; set; }

        public FCProperty(FeatureClassProperty prop, object value)
        {            
            Name = prop.Name;            
            Value = value;
        }

        public static List<FCProperty> GetProperties (FeatureClassPropertyCollection props, ArrayList values)
        {
            List<FCProperty> res = new List<FCProperty>();
            for (int i = 0; i < props.Count; i++)
            {
                FCProperty fcp = new FCProperty(props[i], values[i]);
                res.Add(fcp);
            }            
            return res;
        }
    }
}
