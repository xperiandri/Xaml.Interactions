using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// An interface that a given object must implement in order to be 
    /// set on a ConditionBehavior.Condition property. 
    /// </summary>
    public interface ICondition
    {
        bool Evaluate();
    }
}
