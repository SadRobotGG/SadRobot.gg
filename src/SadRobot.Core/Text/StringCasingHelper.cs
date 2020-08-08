using System.Text;

namespace SadRobot.Core.Text
{
    public static class StringCasingHelper
    {
        /// <summary>
        /// Converts to snake case e.g. from <c>"SnakeCase"</c> to <c>"snake_case"</c>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToSnakeCase(string value) => ToSeparatedCase(value, '_');

        /// <summary>
        /// Converts to kebab case e.g. from <c>"SnakeCase"</c> to <c>"snake-case"</c>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToKebabCase(string value) => ToSeparatedCase(value, '-');

        enum SeparatedCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }

        /// <summary>
        /// Converts camel case or pascal case to a delimited case with
        /// a specified separator
        /// </summary>
        /// <remarks>Based on Newtonsoft.Json.Utilities.StringUtils.ToSeparatedCase</remarks>
        /// <param name="value"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ToSeparatedCase(string value, char separator)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var sb = new StringBuilder();
            var state = SeparatedCaseState.Start;

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == ' ')
                {
                    if (state != SeparatedCaseState.Start) state = SeparatedCaseState.NewWord;
                }
                else if (char.IsUpper(value[i]))
                {
                    switch (state)
                    {
                        case SeparatedCaseState.Upper:
                            var hasNext = i + 1 < value.Length;
                            if (i > 0 && hasNext)
                            {
                                var nextChar = value[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != separator)
                                {
                                    sb.Append(separator);
                                }
                            }
                            break;
                        case SeparatedCaseState.Lower:
                        case SeparatedCaseState.NewWord:
                            sb.Append(separator);
                            break;
                    }

                    var c = char.ToLowerInvariant(value[i]);

                    sb.Append(c);

                    state = SeparatedCaseState.Upper;
                }
                else if (value[i] == separator)
                {
                    sb.Append(separator);
                    state = SeparatedCaseState.Start;
                }
                else
                {
                    if (state == SeparatedCaseState.NewWord) sb.Append(separator);
                    sb.Append(value[i]);
                    state = SeparatedCaseState.Lower;
                }
            }

            return sb.ToString();
        }

    }
}
