using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Omu.ValueInjecter;

namespace FaceAPI.Web.Common.Injection
{
    internal class InjectionForDictionaryToObject : KnownSourceValueInjection<IDictionary<string, object>>
    {
        protected override void Inject(IDictionary<string, object> source, object target)
        {
            foreach (PropertyDescriptor t in target.GetProps())
            {
                if (!source.ContainsKey(t.Name))
                    continue;
                t.SetValue(target, source[t.Name]);
            }
        }
    }
}