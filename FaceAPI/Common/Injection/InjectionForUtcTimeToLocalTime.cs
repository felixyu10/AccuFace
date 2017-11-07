using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;

namespace FaceAPI.Web.Common.Injection
{
    public class InjectionForUtcTimeToLocalTime : ConventionInjection
    {
        private double _time;

        public InjectionForUtcTimeToLocalTime(double time)
        {
            //_time = Convert.ToDouble(time);
            _time = time;
        }

        protected override bool Match(ConventionInfo c)
        {
            return (c.SourceProp.Type == typeof(DateTime) ||
                    c.SourceProp.Type == typeof(DateTime?)) &&
                    c.SourceProp.Name == c.TargetProp.Name &&
                   (c.TargetProp.Type == typeof(DateTime) ||
                    c.TargetProp.Type == typeof(DateTime?));
        }

        protected override object SetValue(ConventionInfo c)
        {
            if (c.SourceProp.Value == null) 
                return null;
            var dt = (DateTime) c.SourceProp.Value;

            try
            {
                return dt.AddHours(_time);
            }
            catch
            {
                return dt;
            }
        }
    }
}
