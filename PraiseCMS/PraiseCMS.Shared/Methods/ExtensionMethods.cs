using Newtonsoft.Json;
//using PraiseCMS.API.Models;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PraiseCMS.Shared.Methods
{
    public static class ExtensionMethods
    {
        private const string KeyString = "G806C8DF278CD5931069B522E695D4F2";

        public static bool IsValidNumber(this string number)
        {
            if (number.StartsWith("+1-") || number.StartsWith("1-") || number.StartsWith("001-"))
            {
                return true;
            }

            number = number.Replace("-", string.Empty).Replace("_", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace(" ", string.Empty);

            return number.Length == 10;
        }

        public static bool EqualsIgnoreCase(this string source, string value)
        {
            return !source.IsNullOrEmpty() && source.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsIgnoreCase(this string source, params string[] strings)
        {
            return strings.Any(x => x.EqualsIgnoreCase(source));
        }

        public static string TrimZeroes(this decimal source)
        {
            return source.ToString().TrimEnd('0').TrimEnd('.');
        }

        public static bool ContainsIgnoreCase(this string source, string value)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            {
                return false;
            }
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string CombineToString(this string[] source, string separator = ",")
        {
            return source != null ? string.Join(separator, source) : string.Empty;
        }

        public static string CombineListToString(this List<string> source, string separator = ",", bool includeWhiteSpace = true)
        {
            // Add a space after the separator if includeWhiteSpace is true
            string separatorWithSpace = includeWhiteSpace ? separator + " " : separator;

            return source != null ? string.Join(separatorWithSpace, source) : string.Empty;
        }

        //This wraps each item in single quotes needed for SQL queries.
        public static string CombineListToSQLString(this List<string> source)
        {
            string result = null;

            foreach (string item in source)
            {
                if (result.IsNull())
                {
                    result = $"'{item}'";
                }
                else
                {
                    result += $",'{item}'";
                }
            }

            return result;
        }

        public static List<string> StringToList(this string source, char separator = ',')
        {
            return source.IsNullOrEmpty() ? null : source.Split(separator).ToList();
        }

        public static string GetLastCharacters(this string source, int count)
        {
            if (!string.IsNullOrEmpty(source) && source.Length >= count)
            {
                source = source.Substring(source.Length - count, count);
            }

            return source;
        }

        public static string GetFirstCharacters(this string source, int count)
        {
            if (!string.IsNullOrEmpty(source) && source.Length >= count)
            {
                source = source.Substring(0, count);
            }

            return source;
        }

        public static string RemoveCharacters(this string source, params char[] charactersToRemove)
        {
            StringBuilder result = new StringBuilder(source.ToLower().Trim());

            foreach (char character in charactersToRemove)
            {
                result.Replace(character.ToString(), string.Empty);
            }

            return result.ToString();
        }

        public static string HtmlFriendly(this string source)
        {
            return source.ToLower().Trim().RemoveCharacters('-', ' ', '?', '/', '\\', '.', ',', ';', '!', '&', '*', '%', '#', '@', '(', ')', '+', '=', '\'', '"', '[', ']');
        }

        public static string PhoneFriendly(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            // Remove all non-numeric characters
            return Regex.Replace(source, @"[^\d]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1.5));
        }

        public static string FilenameFriendlyLower(this string source)
        {
            return source.ToLower().Trim().RemoveCharacters(' ', '?', '/', '\\', '.', ',', ';', '!', '&', '*', '%', '#', '@', '(', ')', '+', '=', '\'', '"', '[', ']');
        }

        public static string FilenameFriendly(this string source)
        {
            return source.RemoveCharacters(' ', '?', '/', '\\', '.', ',', ';', '!', '&', '*', '%', '#', '@', '(', ')', '+', '=', '\'', '"', '[', ']');
        }

        public static bool Between(this DateTime source, DateTime lower, DateTime higher)
        {
            return source >= lower && source <= higher;
        }

        public static DateTime Get12AMVersion(this DateTime source)
        {
            return source.Date;
        }

        public static DateTime Get1159Version(this DateTime source)
        {
            return source.Date.AddDays(1).AddMinutes(-1);
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime dt)
        {
            return StartOfMonth(dt).AddMonths(1).AddDays(-1);
        }

        public static string ToTitleCase(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? str : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();

            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string SubstringIt(this string source, int length)
        {
            return !string.IsNullOrEmpty(source) && source.Length > length ? source.Substring(0, length) : source;
        }

        public static string EllipsisAt(this string source, int length)
        {
            if ((!string.IsNullOrEmpty(source) && source.Length < length) || string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source.SubstringIt(length) + "...";
        }

        public static IEnumerable<string> SplitToList(this string source, char splitter = ',')
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                return source.Trim().Split(splitter).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim());
            }

            return new List<string>();
        }

        public static string ReplaceLastOccurrence(this string source, string find, string replace)
        {
            if (source.EndsWith(find, StringComparison.OrdinalIgnoreCase))
            {
                int place = source.LastIndexOf(find);

                return place == -1 ? source : source.Remove(place, find.Length).Insert(place, replace);
            }

            return source;
        }

        public static string RelativeToNow(this DateTime dt)
        {
            const int second = 1;
            const int minute = 60 * second;
            const int hour = 60 * minute;
            const int day = 24 * hour;
            const int month = 30 * day;

            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * minute)
            {
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }

            if (delta < 2 * minute)
            {
                return "a minute ago";
            }

            if (delta < 45 * minute)
            {
                return ts.Minutes + " minutes ago";
            }

            if (delta < 90 * minute)
            {
                return "an hour ago";
            }

            if (delta < 24 * hour)
            {
                return ts.Hours + " hours ago";
            }

            if (delta < 48 * hour)
            {
                return "yesterday";
            }

            if (delta < 30 * day)
            {
                return ts.Days + " days ago";
            }

            if (delta < 12 * month)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));

                return months <= 1 ? "one month ago" : months + " months ago";
            }

            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));

            return years <= 1 ? "one year ago" : years + " years ago";
        }

        public static string CleanLargeNumber(this int number)
        {
            int i = (int)Math.Pow(10, (int)Math.Max(0, Math.Log10(number) - 2));
            number = number / i * i;

            if (number >= 1000000000)
            {
                return (number / 1000000000D).ToString("0.##") + "B";
            }

            if (number >= 1000000)
            {
                return (number / 1000000D).ToString("0.##") + "M";
            }

            if (number >= 1000)
            {
                return (number / 1000D).ToString("0.##") + "K";
            }

            return number.ToString("#,0");
        }

        public static decimal CalculateDiscount(this decimal source, decimal amount)
        {
            decimal discount = amount / 100.00M * source;

            return source - discount;
        }

        public static string Pluralize(this int source, string word)
        {
            if (source == 1)
            {
                return word;
            }

            if (word.EndsWith("ay", StringComparison.OrdinalIgnoreCase))
            {
                return word + "s";
            }

            if (word.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                return word.ReplaceLastOccurrence("y", "ies");
            }

            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                return word;
            }

            if (word.EndsWith("our", StringComparison.OrdinalIgnoreCase))
            {
                return word + "s";
            }

            return word + "s";
        }

        public static string Pluralize(this decimal source, string word)
        {
            if (source == 1.00M)
            {
                return word;
            }

            if (word.EndsWith("ay", StringComparison.OrdinalIgnoreCase))
            {
                return word + "s";
            }

            if (word.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                return word.ReplaceLastOccurrence("y", "ies");
            }

            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                return word;
            }

            if (word.EndsWith("our", StringComparison.OrdinalIgnoreCase))
            {
                return word + "s";
            }

            return word + "s";
        }

        public static DateRange GetCurrentYearDateRange()
        {
            return new DateRange
            {
                StartDate = DateTime.Parse($"01/01/{DateTime.Now.Year}"),
                EndDate = DateTime.Parse($"12/31/{DateTime.Now.Year}")
            };
        }

        public static DateRange GetLastMonthDateRange()
        {
            return new DateRange
            {
                StartDate = DateTime.Now.AddMonths(-1).Date,
                EndDate = DateTime.Now.Date
            };
        }

        public static DateTime GetNextWeekday(this DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        public static List<DateTime> GetDateList(DateTime start, DateTime end, int everyCount)
        {
            List<DateTime> dateList = new List<DateTime>();

            while (start.Date <= Convert.ToDateTime(end.Date).Date)
            {
                DateTime date = start.AddDays(7 * everyCount);

                if (date.Date <= end)
                {
                    dateList.Add(start.AddDays(7 * everyCount));
                }

                start = start.AddDays(7 * everyCount);
            }

            return dateList;
        }

        public static List<DateTime> GetDateListByMonth(DateTime start, DateTime end, int everyCount)
        {
            List<DateTime> dateList = new List<DateTime>();

            while (start.Date <= Convert.ToDateTime(end.Date))
            {
                if (!dateList.Any())
                {
                    dateList.Add(start);
                    start = start.AddMonths(everyCount);
                    continue;
                }

                DateTime date = start.AddDays(everyCount);

                if (date.Date <= end.Date)
                {
                    dateList.Add(date);
                }

                start = start.AddMonths(everyCount);
            }

            return dateList;
        }

        public static List<DateTime> GetDates(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(year, month, day)) // Map each day to a date
                             .ToList(); // Load dates into a list
        }

        public static List<DateTime> GetWeekDaysList(DateTime start, DateTime end)
        {
            List<DateTime> daysList = new List<DateTime>();

            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (!date.DayOfWeek.Equals(DayOfWeek.Sunday) && !date.DayOfWeek.Equals(DayOfWeek.Saturday))
                {
                    daysList.Add(date);
                }
            }

            return daysList;
        }

        public static List<DateTime> GetDateListForMonthCustomFrequency(DateTime start, DateTime end, string frequency, int everyCount)
        {
            List<DateTime> returnDateList = new List<DateTime>();
            List<DateTime> dateList = GetDateListByMonth(start, end, everyCount);
            int week = start.GetDayNumberOfMonth();
            DayOfWeek weekDay = start.DayOfWeek;
            int startDate = start.Day;

            foreach (DateTime date in dateList)
            {
                List<DateTime> dates = GetDates(date.Year, date.Month);

                if (frequency.Equals(MonthlyCustomFrequency.WeekDay))
                {
                    DateTime nextDate = dates.FirstOrDefault(x => x.GetDayNumberOfMonth().Equals(week) && x.DayOfWeek.Equals(weekDay));

                    if (nextDate.IsNotNullOrEmpty() && nextDate.Year > 2000)
                    {
                        returnDateList.Add(nextDate);
                    }
                }
                else if (frequency.Equals(MonthlyCustomFrequency.MonthDay))
                {
                    DateTime nextDate = dates.FirstOrDefault(x => x.Day.Equals(startDate));

                    if (nextDate.IsNotNullOrEmpty() && nextDate.Year > 2000)
                    {
                        returnDateList.Add(nextDate);
                    }
                }
            }

            return returnDateList;
        }

        public static List<List<DateTime>> GetDatesOfWeeks(DateTime date, int numberOfWeeks, bool includeCurrentWeek = true, string firstDay = "")
        {
            var weeks = new List<List<DateTime>>();
            var startOfWeek = GetStartOfWeek(date, firstDay);

            int totalWeeks = includeCurrentWeek ? numberOfWeeks : numberOfWeeks + 1;

            for (int i = totalWeeks - 1; i >= 0; i--)
            {
                var weekStart = startOfWeek.AddDays(-(i * 7));
                weeks.Add(Enumerable.Range(0, 7).Select(d => weekStart.AddDays(d)).ToList());
            }

            return weeks;
        }

        private static DateTime GetStartOfWeek(DateTime date, string firstDay)
        {
            DayOfWeek firstDayOfWeek = firstDay.IsNotNullOrEmpty() ? (DayOfWeek)Enum.Parse(typeof(DayOfWeek), firstDay) : CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int offset = (int)date.DayOfWeek - (int)firstDayOfWeek;
            if (offset < 0)
            {
                offset += 7;
            }
            return date.AddDays(-offset).Date;
        }

        public static List<DateTime> GetDatesOfWeek(DateTime date, string firstDay = "")
        {
            DayOfWeek firstDayOfWeek = firstDay.IsNotNullOrEmpty() ? (DayOfWeek)Enum.Parse(typeof(DayOfWeek), firstDay) : CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

            int offset = (int)date.DayOfWeek - (int)firstDayOfWeek;
            if (offset < 0)
            {
                offset += 7;
            }
            DateTime startOfWeek = date.AddDays(-offset).Date;

            return Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();
        }

        public static List<List<DateTime>> GetDatesOfLastNumberOfWeeks(DateTime date, int count, bool includeCurrentWeek = true, string firstDay = "")
        {
            var weeks = new List<List<DateTime>>();
            var startOfWeek = GetStartOfWeek(date, firstDay);

            // Calculate the total number of weeks to fetch
            int totalWeeks = includeCurrentWeek ? count : count;

            // Determine the start date of the first week
            var firstWeekStart = startOfWeek.AddDays(-(includeCurrentWeek ? (totalWeeks - 1) * 7 : totalWeeks * 7));

            for (int i = 0; i < totalWeeks; i++)
            {
                var weekStart = firstWeekStart.AddDays(i * 7);
                weeks.Add(Enumerable.Range(0, 7).Select(d => weekStart.AddDays(d)).ToList());
            }

            return weeks;
        }

        public static int GetDayNumberOfMonth(this DateTime date)
        {
            date = date.Date;
            int count = 0;
            string currentDay = date.DayOfWeek.ToString();

            for (int i = 1; i <= date.Day; i++)
            {
                if (new DateTime(date.Year, date.Month, i).DayOfWeek.ToString().EqualsIgnoreCase(currentDay))
                {
                    count++;
                }
            }

            return count;
        }

        public static string GetWorkWeekStartDay(string workWeek)
        {
            // Check if workWeek is not null or empty
            if (!string.IsNullOrEmpty(workWeek))
            {
                // Return the first part of the split workWeek string
                return workWeek.Split('-')[0];
            }

            // Return the default work week if workWeek is null or empty
            // Split the default value to ensure it returns the same type of data
            return WorkWeeks.SundaySaturday.Split('-')[0];
        }

        #region Null Checks
        public static bool IsNotNull(this object s)
        {
            return s != null;
        }

        public static bool IsNotNullOrEmpty(this object s)
        {
            return !s.IsNullOrEmpty();
        }

        public static bool IsNull(this object s)
        {
            return s == null;
        }

        public static bool IsNullOrEmpty(this object s)
        {
            if (s == null) return true;

            if (s is string str)
            {
                return string.IsNullOrEmpty(str.Trim());
            }

            return false;
        }

        public static bool IsNullOrEmptyOrDbNull(this object objectToCheck)
        {
            return objectToCheck.IsNullOrEmpty() || objectToCheck == DBNull.Value;
        }

        public static bool IsNotNullOrEmptyOrDbNull(this object objectToCheck)
        {
            return !objectToCheck.IsNullOrEmptyOrDbNull();
        }
        #endregion

        public static T AppSetting<T>(this string key, T defaultValue)
        {
            return ConfigurationManager.AppSettings[key].IsNullOrEmpty() ?
            defaultValue : (T)Convert.ChangeType((object)ConfigurationManager.AppSettings[key], typeof(T));
        }

        public static string TimeAgo(this DateTime? dateTime)
        {
            return ConvertToTimeAgo(Convert.ToDateTime(dateTime));
        }

        public static string TimeAgo(this DateTime dateTime)
        {
            return ConvertToTimeAgo(dateTime);
        }

        private static string ConvertToTimeAgo(DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                return $"{timeSpan.Seconds} seconds ago";
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                return timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} minutes ago" : "One minute ago";
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                return timeSpan.Hours > 1 ? $"{timeSpan.Hours} hours ago" : "One hour ago";
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                return timeSpan.Days > 1 ? $"{timeSpan.Days} days ago" : "Yesterday";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                return timeSpan.Days > 30 ? $"{timeSpan.Days / 30} months ago" : "One month ago";
            }
            else
            {
                return timeSpan.Days > 365 ? $"{timeSpan.Days / 365} years ago" : "One year ago";
            }
        }

        public static bool CheckURLIsValid(string strURL)
        {
            if (strURL.IsNullOrEmpty())
            {
                return false;
            }

            const string pattern = @"^(http|https|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return reg.IsMatch(strURL);
        }

        public static string CleanUrl(string url)
        {
            if (url.IsNullOrEmpty())
            {
                return url;
            }

            if (!url.StartsWith(URLPrefixes.Https) && !url.StartsWith(URLPrefixes.Http))
            {
                url = URLPrefixes.Http + url;
            }

            return url;
        }

        public static bool CheckEmailIsValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            // Basic email validation pattern
            const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return reg.IsMatch(email);
        }

        public static List<dynamic> ToDynamic(this DataTable dt)
        {
            List<dynamic> dynamicDt = new List<dynamic>();

            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add(dyn);

                foreach (DataColumn column in dt.Columns)
                {
                    IDictionary<string, object> dic = dyn;
                    dic[column.ColumnName] = row[column];
                }
            }

            return dynamicDt;
        }

        public static List<T> DataTableToList<T>(this DataTable dt)
        {
            return (from DataRow row in dt.Rows select GetItem<T>(row)).ToList();
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        try
                        {
                            pro.SetValue(obj, dr[column.ColumnName], null);
                        }
                        catch (Exception ex)
                        {
                            ExceptionLogger.LogException(ex);
                        }
                    }
                }
            }

            return obj;
        }

        public static string SplitBefore(this string text, string beforeValue)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string[] split = text.Split(new string[] { beforeValue }, StringSplitOptions.None);

            return split[0];
        }

        public static string SplitAfter(this string text, string afterValue)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string[] split = text.Split(new string[] { afterValue }, StringSplitOptions.None);

            return split.Length > 1 ? split[1] : string.Empty;
        }

        public static List<string> DeserializeToList(this string value)
        {
            if (value == "[\"\"]" || value.IsNullOrEmpty())
            {
                return new List<string>();
            }

            return JsonConvert.DeserializeObject<List<string>>(value);
        }

        public static int ToInt32(this object value)
        {
            try
            {
                double keyDouble = Convert.ToDouble(value);
                double keyRound = Math.Round(keyDouble);

                return Convert.ToInt32(keyRound);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return 0;
            }
        }

        public static string ConvertToJson<T>(this T obj)
        {
            try
            {
                Dictionary<string, object> dictionary = obj.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj, null));

                return JsonConvert.SerializeObject(dictionary);
            }
            catch (Exception ex)
            {
                try
                {
                    ExceptionLogger.LogException(ex);
                    return JsonConvert.SerializeObject(obj.ToString());
                }
                catch (Exception exc)
                {
                    ExceptionLogger.LogException(exc);
                    return null;
                }
            }
        }

        public static string Encrypt(this string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(KeyString, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }

            return clearText;
        }

        public static string Decrypt(this string cipherText)
        {
            // Check if the cipherText contains valid base64 characters
            if (!IsBase64String(cipherText))
            {
                // If the input is not a valid base64 string, return the input as-is
                return cipherText;
            }

            // Replace spaces with '+' to handle URL-encoded strings
            cipherText = cipherText.Replace(" ", "+");

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(KeyString, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }

                return cipherText;
            }
            catch (Exception ex)
            {
                // If decryption fails (e.g., invalid padding, incorrect key), return the input as-is
                //Console.WriteLine($"Decryption failed: {ex.Message}");
                ExceptionLogger.LogException(ex);
                return cipherText;
            }
        }

        private static bool IsBase64String(string s)
        {
            // Check if the input string is a valid base64 string
            try
            {
                byte[] data = Convert.FromBase64String(s);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string DescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : source.ToString();
        }

        public static DateRange SplitDates(this string combinedDates)
        {
            string[] splitDates = combinedDates.Split('_');

            return new DateRange { StartDate = Convert.ToDateTime(splitDates[0]), EndDate = Convert.ToDateTime(splitDates[1]) };
        }

        public static string ToCurrencyString(this int value)
        {
            return value.ToString("C");
        }

        public static string ToCurrencyString(this long value)
        {
            return value.ToString("C");
        }

        public static string ToCurrencyString(this decimal value)
        {
            return value.ToString("C");
        }

        public static string ToNumberString(this int value)
        {
            return value.ToString("N0");
        }

        public static string ToFormattedPercentage(this double percentage)
        {
            if (percentage >= 99.99 || percentage <= 0.01)
            {
                // Round to the nearest whole number and format without decimals
                return $"{Math.Round(percentage)}%";
            }
            else
            {
                // Format with two decimal places
                return $"{percentage:0.00}%";
            }
        }

        #region Date and Time
        public static string ToPlainDate(this DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

        public static List<string> ToMonths(this List<int> months)
        {
            return months.ConvertAll(item => DateTimeFormatInfo.CurrentInfo?.GetAbbreviatedMonthName(item));
        }

        public static string ToMonthName(this int month)
        {
            return DateTimeFormatInfo.CurrentInfo?.GetAbbreviatedMonthName(month);
        }

        public static string ToFullMonthName(this int month)
        {
            return DateTimeFormatInfo.CurrentInfo?.GetMonthName(month);
        }

        public static DateRange ToDateTimeRange(this string dateRange)
        {
            try
            {
                if (!string.IsNullOrEmpty(dateRange))
                {
                    if (dateRange.Contains("-"))
                    {
                        string[] splitDateRange = dateRange.Split('-');

                        return new DateRange
                        {
                            StartDate = Convert.ToDateTime(splitDateRange[0]),
                            EndDate = Convert.ToDateTime(splitDateRange[1])
                        };
                    }
                    else
                    {
                        return new DateRange
                        {
                            StartDate = Convert.ToDateTime(dateRange),
                            EndDate = Convert.ToDateTime(dateRange).AddDays(1).AddSeconds(-1)
                        };
                    }
                }
                else
                {
                    return new DateRange
                    {
                        StartDate = DateTime.MinValue,
                        EndDate = DateTime.MaxValue
                    };
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return null;
            }
        }

        public static DateRange ToDateRange(this string dateRange)
        {
            try
            {
                if (!string.IsNullOrEmpty(dateRange))
                {
                    if (dateRange.Contains("-"))
                    {
                        string[] splitDateRange = dateRange.Split('-');

                        return new DateRange
                        {
                            StartDate = Convert.ToDateTime(splitDateRange[0]).Date,
                            EndDate = Convert.ToDateTime(splitDateRange[1]).Date
                        };
                    }
                    else
                    {
                        var date = Convert.ToDateTime(dateRange).Date;
                        return new DateRange
                        {
                            StartDate = date,
                            EndDate = date // set as the same as start date because the dateRange most likely only has one date being passed in (like today or yesterday)
                        };
                    }
                }
                else
                {
                    return new DateRange
                    {
                        StartDate = DateTime.MinValue.Date,
                        EndDate = DateTime.MaxValue.Date
                    };
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return null;
            }
        }

        public static DateRange ConvertISOToDateRange(this string isoDateRange)
        {
            try
            {
                if (!string.IsNullOrEmpty(isoDateRange))
                {
                    // Split the ISO 8601 date range string by the comma (or any other delimiter that separates two dates)
                    string[] splitDateRange = isoDateRange.Split(',');

                    if (splitDateRange.Length == 2)
                    {
                        // Parse the ISO 8601 dates using DateTimeOffset to handle the time zone offsets

                        if (DateTimeOffset.TryParse(splitDateRange[0], out DateTimeOffset startDateTimeOffset) &&
                            DateTimeOffset.TryParse(splitDateRange[1], out DateTimeOffset endDateTimeOffset))
                        {
                            return new DateRange
                            {
                                StartDate = startDateTimeOffset.DateTime, // Convert to DateTime if needed
                                EndDate = endDateTimeOffset.DateTime
                            };
                        }
                        else
                        {
                            throw new FormatException("Invalid ISO 8601 date format.");
                        }
                    }
                    else
                    {
                        // Handle the case where only one date is passed
                        if (DateTimeOffset.TryParse(isoDateRange, out DateTimeOffset singleDateOffset))
                        {
                            return new DateRange
                            {
                                StartDate = singleDateOffset.DateTime,
                                EndDate = singleDateOffset.DateTime
                            };
                        }
                        else
                        {
                            throw new FormatException("Invalid ISO 8601 date format.");
                        }
                    }
                }
                else
                {
                    // Return a default DateRange if the input string is empty or null
                    return new DateRange
                    {
                        StartDate = DateTime.MinValue.Date,
                        EndDate = DateTime.MaxValue.Date
                    };
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return null;
            }
        }

        public static string ConvertISODateRangeToCustomFormat(this string isoDateRange)
        {
            try
            {
                // Convert the ISO date range to DateRange using the existing method
                DateRange dateRange = isoDateRange.ConvertISOToDateRange();

                if (dateRange != null)
                {
                    // Format both the start and end dates as "MM/dd/yyyy"
                    string formattedStartDate = dateRange.StartDate.ToString("MM/dd/yyyy");
                    string formattedEndDate = dateRange.EndDate.ToString("MM/dd/yyyy");

                    // Return the formatted date range string
                    return $"{formattedStartDate}-{formattedEndDate}";
                }
                else
                {
                    throw new Exception("DateRange conversion returned null.");
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return null;
            }
        }

        public static DateRange ToDateRange(this DateTime date)
        {
            return new DateRange
            {
                StartDate = date.Date,
                EndDate = date.Date // Same start and end date since it's a single date
            };
        }

        public static string ToFourDigitYear(string year)
        {
            if (!string.IsNullOrEmpty(year) && year.Length == 2)
            {
                year = year.Insert(0, "20");
            }

            return year;
        }

        public static List<string> ToShortDate(this List<DateTime> dates)
        {
            List<string> result = new List<string>();
            List<int> years = dates.ConvertAll(x => x.Date.Year);

            foreach (DateTime item in dates)
            {
                if (years.Any(x => x < DateTime.Now.Year) || years.Any(x => x > DateTime.Now.Year))
                {
                    result.Add(item.ToString("MMM yy"));
                }
                else
                {
                    result.Add(item.ToString("MMM"));
                }
            }

            return result;
        }

        public static string ToShortDateString(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToShortDateString() : "No Date";
        }

        public static string ToShortDateYearString(this DateTime dateTime)
        {
            return dateTime.ToString("MMM dd");
        }

        public static string ToShortDateAndTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy hh:mm tt");
        }

        public static string ToShortDateAndShortTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture);
        }

        public static string ToDateAndShortTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("ddd, MMM d, h:mm tt", CultureInfo.InvariantCulture);
        }

        public static string ToIso8601DateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string ToIso8601DateTimeString(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        public static DateTime GetRoundedTime(DateTime time)
        {
            // Round the time to the nearest 30-minute interval
            int minutes = time.Minute;
            int nextInterval = (minutes < 30) ? 30 : 60;  // Decide if the current minute is rounded to 30 or 60

            // Add the difference to the current time to get the rounded time
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0).AddMinutes(nextInterval);
        }

        #endregion

        //#region Nuvei API Responses
        //public static string GetApiTransactionResponse(string resultCode)
        //{
        //    return resultCode == Constants.ApiTransactionSuccessCode || resultCode == APIStatuses.Success
        //        ? APIStatuses.Success
        //        : APIStatuses.Error;
        //}

        //public static string HandleApiTransactionFailure(ResultModel response)
        //{
        //    List<string> errorMessages = new List<string>();

        //    if (!string.IsNullOrEmpty(response?.result_message))
        //    {
        //        errorMessages.Add(response.result_message);
        //    }

        //    if (!string.IsNullOrEmpty(response?.result_description))
        //    {
        //        errorMessages.Add(response.result_description);
        //    }

        //    if (!string.IsNullOrEmpty(response?.result_message_1))
        //    {
        //        errorMessages.Add(response.result_message_1);
        //    }

        //    if (!string.IsNullOrEmpty(response?.result_message_2))
        //    {
        //        errorMessages.Add(response.result_message_2);
        //    }

        //    // Concatenate error messages with a separator
        //    return string.Join(" | ", errorMessages);
        //}
        //#endregion
    }
}