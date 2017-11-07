using System;
using System.ComponentModel;
using Omu.ValueInjecter;

namespace FaceAPI.Web.Common.Injection
{
    public class InjectionForNotNullField : ValueInjection
    {
        protected override void Inject(object source, object target)
        {
            var tprops = target.GetProps();
            foreach (PropertyDescriptor s in source.GetProps())
            {
                var changeValue = s.GetValue(source);
                if (changeValue == null)
                    continue;

                //value type
                if (s.PropertyType.IsValueType)
                {
                    var vv = Activator.CreateInstance(s.PropertyType);
                    if (changeValue == vv)
                        continue;
                }

                var tp = tprops.GetByName(s.Name);
                if (tp == null)
                    continue;
                tp.SetValue(target, s.GetValue(source));
            }
        }
    }
}