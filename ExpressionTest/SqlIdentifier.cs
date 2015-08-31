using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class SqlIdentifier
    {
        private static SqlCommandBuilder builder = new SqlCommandBuilder();
        private const string ParameterPrefix = "@";
        private const string QuotePrefix = "[";
        private const string QuoteSuffix = "]";
        private const string SchemaSeparator = ".";
        private const char SchemaSeparatorChar = '.';

        private static bool IsQuoted(string s)
        {
            if (s == null)
                throw Error.ArgumentNull("s");
            if (s.Length < 2 || !s.StartsWith("[", StringComparison.Ordinal))
                return false;
            return s.EndsWith("]", StringComparison.Ordinal);
        }

        internal static string QuoteCompoundIdentifier(string s)
        {
            if (s == null)
                throw Error.ArgumentNull("s");
            if (s.StartsWith("@", StringComparison.Ordinal) || SqlIdentifier.IsQuoted(s))
                return s;
            if (!s.StartsWith("[", StringComparison.Ordinal) && s.EndsWith("]", StringComparison.Ordinal))
            {
                int length = s.IndexOf('.');
                if (length < 0)
                    return SqlIdentifier.builder.QuoteIdentifier(s);
                string s1 = s.Substring(0, length);
                string str = s.Substring(length + 1, s.Length - length - 1);
                if (!SqlIdentifier.IsQuoted(str))
                    str = SqlIdentifier.builder.QuoteIdentifier(str);
                return SqlIdentifier.QuoteCompoundIdentifier(s1) + ((string)(object)'.' + (object)str);
            }
            if (s.StartsWith("[", StringComparison.Ordinal) && !s.EndsWith("]", StringComparison.Ordinal))
            {
                int length = s.LastIndexOf('.');
                if (length < 0)
                    return SqlIdentifier.builder.QuoteIdentifier(s);
                string str = s.Substring(0, length);
                if (!SqlIdentifier.IsQuoted(str))
                    str = SqlIdentifier.builder.QuoteIdentifier(str);
                string s1 = s.Substring(length + 1, s.Length - length - 1);
                return str + (object)'.' + SqlIdentifier.QuoteCompoundIdentifier(s1);
            }
            int length1 = s.IndexOf('.');
            if (length1 < 0)
                return SqlIdentifier.builder.QuoteIdentifier(s);
            return SqlIdentifier.QuoteCompoundIdentifier(s.Substring(0, length1)) + (object)'.' + SqlIdentifier.QuoteCompoundIdentifier(s.Substring(length1 + 1, s.Length - length1 - 1));
        }

        internal static string QuoteIdentifier(string s)
        {
            if (s == null)
                throw Error.ArgumentNull("s");
            if (s.StartsWith("@", StringComparison.Ordinal) || SqlIdentifier.IsQuoted(s))
                return s;
            return SqlIdentifier.builder.QuoteIdentifier(s);
        }

        internal static IEnumerable<string> GetCompoundIdentifierParts(string s)
        {
            if (s == null)
                throw Error.ArgumentNull("s");
            if (s.StartsWith("@", StringComparison.Ordinal))
                throw Error.ArgumentWrongValue((object)"s");
            Match match = Regex.Match(SqlIdentifier.QuoteCompoundIdentifier(s), "^(?<component>\\[([^\\]]|\\]\\])*\\])(\\.(?<component>\\[([^\\]]|\\]\\])*\\]))*$");
            if (!match.Success)
                throw Error.ArgumentWrongValue((object)"s");
            foreach (Capture capture in match.Groups["component"].Captures)
                yield return capture.Value;
        }
    }
}
