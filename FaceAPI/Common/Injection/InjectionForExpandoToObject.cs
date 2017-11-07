using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;

namespace FaceAPI.Web.Common.Injection
{
    internal class InjectionForExpandoToObject : KnownSourceValueInjection<ExpandoObject>
    {
        protected override void Inject(ExpandoObject source, object target)
        {
            var d = source as IDictionary<string, object>;
            if (d == null) return;
            var tprops = target.GetProps();

            foreach (var o in d)
            {
                var tp = tprops.GetByName(o.Key);
                if (tp == null)
                    continue;

                object v = o.Value.ToGetValueOrDefault(tp.PropertyType);

                //民國年
                //if (tp.PropertyType == typeof(DateTime) || tp.PropertyType == typeof(DateTime?))
                //{
                //    var tV = o.Value.ToGetValueOrDefault("");
                //    if (tV.Length == 9)//102-01-01
                //    {
                //        var arr = tV.Split('-');
                //        var year = arr[0].ToGetValueOrDefault(0) + 1911;
                //        var month = arr[1].ToGetValueOrDefault(0);
                //        var day = arr[2].ToGetValueOrDefault(0);
                //        v = new DateTime(year, month, day);
                //    }
                //}

                tp.SetValue(target, v);
            }
        }
    }
}
