using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using System.ComponentModel;

namespace Accupass.Common.Injection
{
    public class InjectionForDB : ValueInjection
    {
        protected override void Inject(object source, object target)
        {
            var restrictedStrArr = new[]
            {
                "Id",
                "CreateDatetime",
                "CreateUser_Id", 
                "UpdateDatetime", 
                "UpdateUser_Id",
                "IdNumber"
            };

            var tprops = target.GetProps();
            foreach (PropertyDescriptor s in source.GetProps())
            {
                if (restrictedStrArr.Contains(s.Name))
                    continue;
                var tp = tprops.GetByName(s.Name);
                if (tp == null)
                    continue;
                tp.SetValue(target, s.GetValue(source));
            }
        }
    }
}
