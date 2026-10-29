using UnityEngine;

namespace Assets.Extensions
{
    public enum ComparisonOp { Greater, GreaterOrEqual, Less, LessOrEqual, Equal, NotEqual, Mask }

    public static class ComparisonExtensions
    {
        public static bool EvaluateOp(this ComparisonOp op, float value, float threshold, float epsilon = 0.001f)
        {
            switch (op)
            {
                case ComparisonOp.Greater: return value > threshold;
                case ComparisonOp.GreaterOrEqual: return value >= threshold;
                case ComparisonOp.Less: return value < threshold;
                case ComparisonOp.LessOrEqual: return value <= threshold;
                case ComparisonOp.Equal: return Mathf.Abs(value - threshold) <= epsilon;
                case ComparisonOp.NotEqual: return Mathf.Abs(value - threshold) > epsilon;
                case ComparisonOp.Mask: return ((int)value & (int)threshold) != 0;
                default: return false;
            }
        }

        public static string GetFlavorText(this ComparisonOp comparisonOp)
        {
            return comparisonOp switch
            {
                ComparisonOp.Greater => "More than ",
                ComparisonOp.GreaterOrEqual => "at least ",
                ComparisonOp.Less => "Less than ",
                ComparisonOp.LessOrEqual => "at most ",
                ComparisonOp.Equal => "",
                ComparisonOp.NotEqual => "not ",
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}