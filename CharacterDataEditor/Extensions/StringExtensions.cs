﻿using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Extensions
{
    public static class StringExtensions
    {
        public static string AddSpacesToCamelCase(this string input)
        {
            var spacedString = new string(Enumerable.Concat(
                input.Take(1), 
                InsertSpacesBeforeCapsOrNumbers(input.Skip(1))
                ).ToArray());
            return spacedString;
        }

        public static string UpperCaseFirstLetter(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            if (input.Length == 1)
            {
                return input.ToUpper();
            }

            var firstChar = input[0];

            return $"{firstChar.ToString().ToUpper()}{input.Substring(1)}";
        }

        public static string ToCamelCase(this string input)
        {
            return input.Replace(" ", "");
        }

        private static IEnumerable<char> InsertSpacesBeforeCapsOrNumbers(IEnumerable<char> input)
        {
            foreach (char c in input)
            {
                if (char.IsUpper(c) || char.IsNumber(c))
                {
                    yield return ' ';
                }

                yield return c;
            }
        }
    }
}
