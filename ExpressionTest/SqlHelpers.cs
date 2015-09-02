using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 为与字符串模式匹配的操作提供帮助器方法。
    /// </summary>
    public static class SqlHelpers
    {
        /// <summary>
        /// 创建一个搜索模式字符串，其中，指定文本可包含其前后的其他文本。
        /// </summary>
        /// 
        /// <returns>
        /// 一个搜索模式字符串，它包含指定字符串及其前后的“%”字符。
        /// </returns>
        /// <param name="text">要插入到搜索模式字符串中的字符串。</param><param name="escape">用来转义通配符的字符。</param>
        public static string GetStringContainsPattern(string text, char escape)
        {
            bool usedEscapeChar = false;
            return SqlHelpers.GetStringContainsPattern(text, escape, out usedEscapeChar);
        }

        internal static string GetStringContainsPattern(string text, char escape, out bool usedEscapeChar)
        {
            if (text == null)
                throw Error.ArgumentNull("text");
            return "%" + SqlHelpers.EscapeLikeText(text, escape, false, out usedEscapeChar) + "%";
        }

        internal static string GetStringContainsPatternForced(string text, char escape)
        {
            if (text == null)
                throw Error.ArgumentNull("text");
            bool usedEscapeChar = false;
            return "%" + SqlHelpers.EscapeLikeText(text, escape, true, out usedEscapeChar) + "%";
        }

        /// <summary>
        /// 创建一个搜索模式字符串，其中，指定文本可包含其后面的其他文本，但不能包含其前面的其他文本。
        /// </summary>
        /// 
        /// <returns>
        /// 一个搜索模式字符串，它包含指定字符串及其后面的“%”字符。
        /// </returns>
        /// <param name="text">要插入到搜索模式字符串中的字符串。</param><param name="escape">用来转义通配符的字符。</param>
        public static string GetStringStartsWithPattern(string text, char escape)
        {
            bool usedEscapeChar = false;
            return SqlHelpers.GetStringStartsWithPattern(text, escape, out usedEscapeChar);
        }

        internal static string GetStringStartsWithPattern(string text, char escape, out bool usedEscapeChar)
        {
            if (text == null)
                throw Error.ArgumentNull("text");
            return SqlHelpers.EscapeLikeText(text, escape, false, out usedEscapeChar) + "%";
        }

        internal static string GetStringStartsWithPatternForced(string text, char escape)
        {
            if (text == null)
                throw Error.ArgumentNull("text");
            bool usedEscapeChar = false;
            return SqlHelpers.EscapeLikeText(text, escape, true, out usedEscapeChar) + "%";
        }

        /// <summary>
        /// 创建一个搜索模式字符串，其中，指定文本可包含其前面的其他文本，但不能包含其后面的其他文本。
        /// </summary>
        /// 
        /// <returns>
        /// 一个搜索模式字符串，它包含“%”字符及其后面的指定字符串。
        /// </returns>
        /// <param name="text">要插入到搜索模式字符串中的字符串。</param><param name="escape">用来转义通配符的字符。</param>
        public static string GetStringEndsWithPattern(string text, char escape)
        {
            bool usedEscapeChar = false;
            return SqlHelpers.GetStringEndsWithPattern(text, escape, out usedEscapeChar);
        }

        internal static string GetStringEndsWithPattern(string text, char escape, out bool usedEscapeChar)
        {
            if (text == null)
                throw Error.ArgumentNull("text");
            return "%" + SqlHelpers.EscapeLikeText(text, escape, false, out usedEscapeChar);
        }

        internal static string GetStringEndsWithPatternForced(string text, char escape)
        {
            if (text == null)
                throw Error.ArgumentNull("text");
            bool usedEscapeChar = false;
            return "%" + SqlHelpers.EscapeLikeText(text, escape, true, out usedEscapeChar);
        }

        private static string EscapeLikeText(string text, char escape, bool forceEscaping, out bool usedEscapeChar)
        {
            usedEscapeChar = false;
            if (!forceEscaping && !text.Contains("%") && (!text.Contains("_") && !text.Contains("[")) && !text.Contains("^"))
                return text;
            StringBuilder stringBuilder = new StringBuilder(text.Length);
            foreach (char ch in text)
            {
                switch (ch)
                {
                    case '%':
                    case '_':
                    case '[':
                    case '^':
                        stringBuilder.Append(escape);
                        usedEscapeChar = true;
                        break;
                    default:
                        if ((int)ch != (int)escape)
                            break;
                        goto case '%';
                }
                stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 将 Visual Basic Like 运算符的搜素模式转换为 SQL Server LIKE 运算符的搜索模式。
        /// </summary>
        /// 
        /// <returns>
        /// 与指定的 Visual Basic Like 搜索模式相对应的 SQL Server LIKE 运算符的搜索模式。
        /// </returns>
        /// <param name="pattern">要转换为 SQL Server LIKE 搜索模式的 Visual Basic Like 搜索模式。</param><param name="escape">用于转义特殊 SQL 字符或转义符本身的字符。</param>
        public static string TranslateVBLikePattern(string pattern, char escape)
        {
            if (pattern == null)
                throw Error.ArgumentNull("pattern");
            StringBuilder stringBuilder = new StringBuilder();
            bool flag1 = false;
            bool flag2 = false;
            bool flag3 = false;
            int num = 0;
            foreach (char ch in pattern)
            {
                if (flag1)
                {
                    ++num;
                    if (flag3)
                    {
                        if ((int)ch != 93)
                        {
                            stringBuilder.Append('^');
                            flag3 = false;
                        }
                        else
                        {
                            stringBuilder.Append('!');
                            flag3 = false;
                        }
                    }
                    if ((uint)ch <= 45U)
                    {
                        if ((int)ch != 33)
                        {
                            if ((int)ch == 45)
                            {
                                if (flag2)
                                    throw Error.VbLikeDoesNotSupportMultipleCharacterRanges();
                                flag2 = true;
                                stringBuilder.Append('-');
                                continue;
                            }
                        }
                        else
                        {
                            if (num == 1)
                            {
                                flag3 = true;
                                continue;
                            }
                            stringBuilder.Append(ch);
                            continue;
                        }
                    }
                    else if ((int)ch != 93)
                    {
                        if ((int)ch == 94)
                        {
                            if (num == 1)
                                stringBuilder.Append(escape);
                            stringBuilder.Append(ch);
                            continue;
                        }
                    }
                    else
                    {
                        flag1 = false;
                        flag3 = false;
                        stringBuilder.Append(']');
                        continue;
                    }
                    if ((int)ch == (int)escape)
                    {
                        stringBuilder.Append(escape);
                        stringBuilder.Append(escape);
                    }
                    else
                        stringBuilder.Append(ch);
                }
                else
                {
                    if ((uint)ch <= 42U)
                    {
                        if ((int)ch != 35)
                        {
                            if ((int)ch != 37)
                            {
                                if ((int)ch == 42)
                                {
                                    stringBuilder.Append('%');
                                    continue;
                                }
                                goto label_38;
                            }
                        }
                        else
                        {
                            stringBuilder.Append("[0-9]");
                            continue;
                        }
                    }
                    else if ((int)ch != 63)
                    {
                        if ((int)ch != 91)
                        {
                            if ((int)ch != 95)
                                goto label_38;
                        }
                        else
                        {
                            flag1 = true;
                            flag2 = false;
                            num = 0;
                            stringBuilder.Append('[');
                            continue;
                        }
                    }
                    else
                    {
                        stringBuilder.Append('_');
                        continue;
                    }
                    stringBuilder.Append(escape);
                    stringBuilder.Append(ch);
                    continue;
                    label_38:
                    if ((int)ch == (int)escape)
                    {
                        stringBuilder.Append(escape);
                        stringBuilder.Append(escape);
                    }
                    else
                        stringBuilder.Append(ch);
                }
            }
            if (flag1)
                throw Error.VbLikeUnclosedBracket();
            return stringBuilder.ToString();
        }
    }
}
