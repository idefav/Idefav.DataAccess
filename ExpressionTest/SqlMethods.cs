using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    public static class SqlMethods
    {
        /// <summary>
        /// 对两个不可以为 null 的日期之间的年份边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的年份边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffYear(DateTime startDate, DateTime endDate)
        {
            return endDate.Year - startDate.Year;
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的年份边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的年份边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffYear(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffYear(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的年份边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的年份边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffYear(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffYear(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的年份边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的年份边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffYear(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffYear(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的月份边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的月份边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffMonth(DateTime startDate, DateTime endDate)
        {
            return 12 * (endDate.Year - startDate.Year) + endDate.Month - startDate.Month;
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的月份边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的月份边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffMonth(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffMonth(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的月份边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的月份边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffMonth(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffMonth(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的月份边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的月份边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffMonth(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffMonth(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的日期边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的日期边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffDay(DateTime startDate, DateTime endDate)
        {
            return (endDate.Date - startDate.Date).Days;
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的日期边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的日期边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffDay(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffDay(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的日期边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的日期边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffDay(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffDay(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的日期边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的日期边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffDay(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffDay(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的小时边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的小时边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffHour(DateTime startDate, DateTime endDate)
        {
            return checked(SqlMethods.DateDiffDay(startDate, endDate) * 24 + endDate.Hour - startDate.Hour);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的小时边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的小时边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffHour(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffHour(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的小时边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的小时边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffHour(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffHour(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的小时边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的小时边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffHour(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffHour(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的分钟边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的分钟边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffMinute(DateTime startDate, DateTime endDate)
        {
            return checked(SqlMethods.DateDiffHour(startDate, endDate) * 60 + endDate.Minute - startDate.Minute);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的分钟边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的分钟边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffMinute(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffMinute(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的分钟边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的分钟边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffMinute(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffMinute(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的分钟边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的分钟边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffMinute(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffMinute(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的秒边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffSecond(DateTime startDate, DateTime endDate)
        {
            return checked(SqlMethods.DateDiffMinute(startDate, endDate) * 60 + endDate.Second - startDate.Second);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的秒边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffSecond(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffSecond(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的秒边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffSecond(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffSecond(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的秒边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffSecond(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffSecond(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的毫秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的毫秒边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffMillisecond(DateTime startDate, DateTime endDate)
        {
            return checked(SqlMethods.DateDiffSecond(startDate, endDate) * 1000 + endDate.Millisecond - startDate.Millisecond);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的毫秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的毫秒边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffMillisecond(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffMillisecond(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可以为 null 的日期之间的毫秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的毫秒边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffMillisecond(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffMillisecond(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可以为 null 的日期之间的毫秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的毫秒边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffMillisecond(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffMillisecond(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可为 null 的日期之间的微秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的微秒边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffMicrosecond(DateTime startDate, DateTime endDate)
        {
            return checked((int)unchecked(checked(endDate.Ticks - startDate.Ticks) / 10L));
        }

        /// <summary>
        /// 对两个可为 null 的日期之间的微秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的微秒边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffMicrosecond(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffMicrosecond(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可为 null 的日期之间的微秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的微秒边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffMicrosecond(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffMicrosecond(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可为 null 的日期之间的微秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的微秒边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffMicrosecond(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffMicrosecond(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可为 null 的日期之间的毫微秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的毫微秒边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffNanosecond(DateTime startDate, DateTime endDate)
        {
            return checked((int)((endDate.Ticks - startDate.Ticks) * 100L));
        }

        /// <summary>
        /// 对两个可为 null 的日期之间的毫微秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的毫微秒边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffNanosecond(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffNanosecond(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 对两个不可为 null 的日期之间的毫微秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 两个指定日期之间的毫微秒边界数。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int DateDiffNanosecond(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return SqlMethods.DateDiffNanosecond(startDate.UtcDateTime, endDate.UtcDateTime);
        }

        /// <summary>
        /// 对两个可为 null 的日期之间的毫微秒边界进行计数。
        /// </summary>
        /// 
        /// <returns>
        /// 当两个参数都不为 null 时，返回两个指定日期之间的毫微秒边界数。当一个参数为或两个参数都为 null 时，返回 null 值。
        /// </returns>
        /// <param name="startDate">时间段的起始日期。</param><param name="endDate">时间段的结束日期。</param>
        public static int? DateDiffNanosecond(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return new int?(SqlMethods.DateDiffNanosecond(startDate.Value, endDate.Value));
            return new int?();
        }

        /// <summary>
        /// 确定特定字符串是否与指定的模式匹配。目前只有 LINQ to SQL 查询支持此方法。
        /// </summary>
        /// 
        /// <returns>
        /// 如果 <paramref name="matchExpression"/> 与模式匹配，则为 true；否则为 false。
        /// </returns>
        /// <param name="matchExpression">要搜索其匹配项的字符串。</param><param name="pattern">与 <paramref name="matchExpression"/> 匹配的模式（可能包括通配符）。</param>
        public static bool Like(string matchExpression, string pattern)
        {
            throw Error.SqlMethodOnlyForSql((object)MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// 确定特定字符串是否与指定的模式匹配。目前只有 LINQ to SQL 查询支持此方法。
        /// </summary>
        /// 
        /// <returns>
        /// 如果 <paramref name="matchExpression"/> 与模式匹配，则为 true；否则为 false。
        /// </returns>
        /// <param name="matchExpression">要搜索其匹配项的字符串。</param><param name="pattern">与 <paramref name="matchExpression"/> 匹配的模式（可能包括通配符）。</param><param name="escapeCharacter">放置在通配符前面的字符，以指示应将其解释为常规字符而非通配符。</param>
        public static bool Like(string matchExpression, string pattern, char escapeCharacter)
        {
            throw Error.SqlMethodOnlyForSql((object)MethodBase.GetCurrentMethod());
        }

        internal static int RawLength(string value)
        {
            throw Error.SqlMethodOnlyForSql((object)MethodBase.GetCurrentMethod());
        }

        internal static int RawLength(byte[] value)
        {
            throw Error.SqlMethodOnlyForSql((object)MethodBase.GetCurrentMethod());
        }

        internal static int RawLength(Binary value)
        {
            throw Error.SqlMethodOnlyForSql((object)MethodBase.GetCurrentMethod());
        }
    }
}
