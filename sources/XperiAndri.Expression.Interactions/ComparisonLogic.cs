using XperiAndri.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XperiAndri.Expression.Interactivity
{
    internal static class ComparisonLogic
    {
        /// <summary>
        /// Evaluates both operands that implement the IComparable interface.
        /// </summary>
        /// <param name="leftOperand">Left operand from the LeftOperand property.</param>
        /// <param name="operatorType">Operator from Operator property.</param>
        /// <param name="rightOperand">Right operand from the RightOperand property.</param>
        /// <returns>Returns true if the condition is met; otherwise, returns false.</returns>
        private static bool EvaluateComparable(IComparable leftOperand, ComparisonConditionType operatorType, IComparable rightOperand)
        {
            object obj2 = null;
            try
            {
                obj2 = Convert.ChangeType(rightOperand, leftOperand.GetType(), CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
            }
            catch (InvalidCastException)
            {
            }
            if (obj2 == null)
            {
                return (operatorType == ComparisonConditionType.NotEqual);
            }
            int num = leftOperand.CompareTo((IComparable)obj2);
            switch (operatorType)
            {
                case ComparisonConditionType.Equal:
                    return (num == 0);

                case ComparisonConditionType.NotEqual:
                    return (num != 0);

                case ComparisonConditionType.LessThan:
                    return (num < 0);

                case ComparisonConditionType.LessThanOrEqual:
                    return (num <= 0);

                case ComparisonConditionType.GreaterThan:
                    return (num > 0);

                case ComparisonConditionType.GreaterThanOrEqual:
                    return (num >= 0);
            }
            return false;
        }

        /// <summary>
        /// This method evaluates operands. 
        /// </summary>
        /// <param name="leftOperand">Left operand from the LeftOperand property.</param>
        /// <param name="operatorType">Operator from Operator property.</param>
        /// <param name="rightOperand">Right operand from the RightOperand property.</param>
        /// <returns>Returns true if the condition is met; otherwise, returns false.</returns>
        internal static bool EvaluateImpl(object leftOperand, ComparisonConditionType operatorType, object rightOperand)
        {
            if (leftOperand != null)
            {
                Type type = leftOperand.GetType();
                if (rightOperand != null)
                {
                    //rightOperand = TypeConverterHelper.DoConversionFrom(TypeConverterHelper.GetTypeConverter(type), rightOperand);
                    if (type.GetTypeInfo().IsEnum && Enum.IsDefined(type, rightOperand))
                        rightOperand = Enum.ToObject(type, rightOperand);
                }
            }

            IComparable comparable = leftOperand as IComparable;
            IComparable comparable2 = rightOperand as IComparable;
            if ((comparable != null) && (comparable2 != null))
            {
                return EvaluateComparable(comparable, operatorType, comparable2);
            }
            switch (operatorType)
            {
                case ComparisonConditionType.Equal:
                    return object.Equals(leftOperand, rightOperand);

                case ComparisonConditionType.NotEqual:
                    return !object.Equals(leftOperand, rightOperand);

                case ComparisonConditionType.LessThan:
                case ComparisonConditionType.LessThanOrEqual:
                case ComparisonConditionType.GreaterThan:
                case ComparisonConditionType.GreaterThanOrEqual:
                    if ((comparable == null) && (comparable2 == null))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.InvalidOperands, new object[] { (leftOperand != null) ? leftOperand.GetType().Name : "null", (rightOperand != null) ? rightOperand.GetType().Name : "null", operatorType.ToString() }));
                    }
                    if (comparable == null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.InvalidLeftOperand, new object[] { (leftOperand != null) ? leftOperand.GetType().Name : "null", operatorType.ToString() }));
                    }
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.InvalidRightOperand, new object[] { (rightOperand != null) ? rightOperand.GetType().Name : "null", operatorType.ToString() }));
            }
            return false;
        }
    }
}
