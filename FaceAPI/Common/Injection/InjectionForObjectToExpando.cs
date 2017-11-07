using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using Omu.ValueInjecter;

namespace FaceAPI.Web.Common.Injection
{
    internal class InjectionForObjectToExpando : KnownTargetValueInjection<ExpandoObject>
    {
        protected override void Inject(object source, ref ExpandoObject target)
        {
            var t = target as IDictionary<string, object>;
            if (t == null)
                return;

            foreach (PropertyDescriptor s in source.GetProps())
                t[s.Name] = s.GetValue(source);
        }
    }

    internal class InjectionForObjectToExpandoAPI : KnownTargetValueInjection<ExpandoObject>
    {
        protected override void Inject(object source, ref ExpandoObject target)
        {
            var t = target as IDictionary<string, object>;
            if (t == null)
                return;

            foreach (PropertyDescriptor s in source.GetProps())
            {
                if (s.GetValue(source) == null)
                    continue;

                t[s.Name] = s.GetValue(source);
            }
        }
    }
}