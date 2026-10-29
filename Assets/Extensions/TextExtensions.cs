using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Assets.Extensions
{
    public static class ColorExtensions
    {
        public static string ColorToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        public static string WrapRichTextInColor(this string text, Color color)
        {
            return $"<color=#{color.ColorToHex()}>{text}</color>";
        }
    }

    public static class StringBuilderExtensions
    {
        public static string WrapLines(this string paragraph, int maxCharactersLength)
        {
            var spParagraph = new StringBuilder(paragraph);
            var lastSpace = 0;
            for (int i = 0; i < spParagraph.Length; i++)
            {
                if (i > maxCharactersLength + lastSpace && spParagraph[i].Equals(' '))
                {
                    lastSpace += i - lastSpace + maxCharactersLength;
                    spParagraph[i] = '\n';
                }
            }
            return spParagraph.ToString();
        }
    }


    public static class NumericExtensions
    {
        public static string ToKMB(this int value, int decimalPlaces = 1)
        {
            // Use the invariant culture to ensure consistent formatting (e.g., a period as the decimal separator)
            // regardless of the user's system settings.
            var culture = CultureInfo.InvariantCulture;

            if (value >= 1000000000) // Billions (B)
            {
                return (value / 1000000000F).ToString($"F{decimalPlaces}", culture) + "B";
            }
            if (value >= 1000000) // Millions (M)
            {
                return (value / 1000000F).ToString($"F{decimalPlaces}", culture) + "M";
            }
            if (value >= 1000) // Thousands (K)
            {
                return (value / 1000F).ToString($"F{decimalPlaces}", culture) + "K";
            }

            // For numbers less than 1000, format normally
            return value.ToString($"F{decimalPlaces}", culture);
        }
        public static string ToKMB(this float value, int decimalPlaces = 1)
        {
            var culture = CultureInfo.InvariantCulture;
            if (value >= 1000000000) // Billions (B)
            {
                return (value / 1000000000F).ToString($"F{decimalPlaces}", culture) + "B";
            }
            if (value >= 1000000) // Millions (M)
            {
                return (value / 1000000F).ToString($"F{decimalPlaces}", culture) + "M";
            }
            if (value >= 1000) // Thousands (K)
            {
                return (value / 1000F).ToString($"F{decimalPlaces}", culture) + "K";
            }

            // For numbers less than 1000, format normally
            return value.ToString($"F{decimalPlaces}", culture);
        }

        public static string ToClock(this float value) => TimeSpan.FromSeconds(value).ToString("hh:mm:ss");
        public static string ToRelativeTimeString(this float value)
        {
            var timeSpan = TimeSpan.FromSeconds(value);
            static string Plural(int value) => value == 1 ? "" : "s";
            var result = "";
            if (timeSpan.Hours >= 1)
                result += $"{timeSpan.Hours} Hour{Plural(timeSpan.Hours)} ";
            if (timeSpan.Minutes >= 1)
                result += $"{timeSpan.Minutes} Minute{Plural(timeSpan.Minutes)} ";            
            result += $"{timeSpan.Seconds} Second{Plural(timeSpan.Seconds)} ";

            return result;
        }

    }
}