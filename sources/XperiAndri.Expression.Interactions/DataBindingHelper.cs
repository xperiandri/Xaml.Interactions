using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;

namespace Microsoft.Expression.Interactivity
{
    internal static class DataBindingHelper
    {
        // Fields
        private readonly static Dictionary<Type, IList<DependencyProperty>> DependenciesPropertyCache = new Dictionary<Type, IList<DependencyProperty>>();

        // Methods
        public static void EnsureBindingUpToDate(DependencyObject target, DependencyProperty dp)
        {
            BindingExpression expression = target.ReadLocalValue(dp) as BindingExpression;
            if (expression != null)
            {
                target.ClearValue(dp);
                target.SetValue(dp, expression);
            }
        }

        public static void EnsureDataBindingOnActionsUpToDate(TriggerBase<DependencyObject> trigger)
        {
            foreach (TriggerAction action in trigger.Actions)
            {
                EnsureDataBindingUpToDateOnMembers(action);
            }
        }

        public static void EnsureDataBindingUpToDateOnMembers(DependencyObject dpObject)
        {
            IList<DependencyProperty> list = null;
            if (!DependenciesPropertyCache.TryGetValue(dpObject.GetType(), out list))
            {
                list = new List<DependencyProperty>();
                for (Type type = dpObject.GetType(); type != null; type = type.GetTypeInfo().BaseType)
                {
                    foreach (FieldInfo info in type.GetRuntimeFields())
                    {
                        if (info.IsPublic && (info.FieldType == typeof(DependencyProperty)))
                        {
                            DependencyProperty item = info.GetValue(null) as DependencyProperty;
                            if (item != null)
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
                DependenciesPropertyCache[dpObject.GetType()] = list;
            }
            if (list != null)
            {
                foreach (DependencyProperty property2 in list)
                {
                    EnsureBindingUpToDate(dpObject, property2);
                }
            }
        }
    }
}
