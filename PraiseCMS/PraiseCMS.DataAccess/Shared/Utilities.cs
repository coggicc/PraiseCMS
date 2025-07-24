using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Repository;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using PhoneNumber = Twilio.Types.PhoneNumber;

namespace PraiseCMS.DataAccess.Shared
{
    public static class Utilities
    {
        private static readonly ApplicationDbContext db = new ApplicationDbContext();
        private static readonly AdoDataAccess adoData = new AdoDataAccess();
        private static readonly LogsRepository logsRepository = new LogsRepository();

        // Asynchronous method to get IP address
        public static async Task<string> GetIPAsync()
        {
            const string uri = "http://checkip.dyndns.org/";
            try
            {
                using (var client = new HttpClient())
                {
                    var result = await client.GetStringAsync(uri);
                    var ip = result.Split(':')[1].Split('<')[0];

                    return !string.IsNullOrEmpty(ip) ? ip.Trim() : null;
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return null;
            }
        }

        public static string GetClientIp(HttpRequestBase request)
        {
            string ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ip))
            {
                // X-Forwarded-For can have multiple IPs, take the first one (real client)
                ip = ip.Split(',').First().Trim();
            }
            else
            {
                ip = request.UserHostAddress;
            }

            // Convert IPv6 localhost to IPv4
            return ip == "::1" ? "127.0.0.1" : ip;
        }

        // Static method to fetch blacklisted IPs with an injected DbContext
        public static List<string> GetBlackListedIPs(ApplicationDbContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            return dbContext.IPBlacklist
                .Where(x => !string.IsNullOrEmpty(x.IpAddress))
                .Select(s => s.IpAddress)
                .ToList();
        }

        public static List<string> BlackListedIPs()
        { return db.IPBlacklist.Where(x => !string.IsNullOrEmpty(x.IpAddress)).Select(s => s.IpAddress).ToList(); }

        #region Old Encryption and Decryption        
        //// Used to Change encryption from old version to new. Should not be needed anymore as of 3/12/2024
        //public static string MigrateAndEncrypt(string oldData)
        //{
        //    // Step 1: Decrypt using the old TripleDES encryption
        //    string decryptedData = Decrypt(oldData);

        //    string decryptNew = ExtensionMethods.Decrypt(oldData);
        //    string migratedAndEncryptedData = string.Empty;
        //    // Step 2: Encrypt using the new version
        //    if (!string.IsNullOrEmpty(decryptedData))
        //    {
        //        migratedAndEncryptedData = ExtensionMethods.Encrypt(decryptedData);
        //    }

        //    if (string.IsNullOrEmpty(migratedAndEncryptedData))
        //    {
        //        migratedAndEncryptedData = ExtensionMethods.Encrypt(decryptNew);
        //    }
        //    return migratedAndEncryptedData;
        //}

        //public static string Decrypt(string data)
        //{
        //    if (data.IsNullOrEmpty())
        //    {
        //        return null;
        //    }

        //    var decryptedResult = string.Empty;

        //    try
        //    {
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            var salt = CreateRandomSalt(7);
        //            var desKey = CreateHash("n4phc4r3");
        //            var tdes = new TripleDESCryptoServiceProvider();
        //            var pdb = new PasswordDeriveBytes(desKey, salt);
        //            tdes.Key = pdb.CryptDeriveKey("TripleDES", "MD5", 192, tdes.IV);
        //            tdes.Mode = CipherMode.ECB;
        //            tdes.Padding = PaddingMode.PKCS7;

        //            var buff = Convert.FromBase64String(data);
        //            decryptedResult = Encoding.ASCII.GetString(tdes.CreateDecryptor().TransformFinalBlock(buff, 0, buff.Length));
        //            tdes.Clear();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionLogger.LogException(ex);
        //    }

        //    return decryptedResult;
        //}

        //public static string Encrypt(string data)
        //{
        //    if (data.IsNullOrEmpty())
        //    {
        //        return null;
        //    }

        //    var desKey = CreateHash("n4phc4r3");
        //    var salt = CreateRandomSalt(7);
        //    var tdes = new TripleDESCryptoServiceProvider();
        //    var pdb = new PasswordDeriveBytes(desKey, salt);
        //    tdes.Key = pdb.CryptDeriveKey("TripleDES", "MD5", 192, tdes.IV);
        //    tdes.Mode = CipherMode.ECB;
        //    tdes.Padding = PaddingMode.PKCS7;

        //    var buff = Encoding.ASCII.GetBytes(data);
        //    var encryptedResult = Convert.ToBase64String(tdes.CreateEncryptor().TransformFinalBlock(buff, 0, buff.Length));
        //    tdes.Clear();

        //    return encryptedResult;
        //}

        //private static byte[] CreateRandomSalt(int Length)
        //{
        //    var randBytes = Length >= 1 ? new byte[Length] : new byte[1];
        //    var rand = new RNGCryptoServiceProvider();
        //    rand.GetBytes(randBytes);

        //    return randBytes;
        //}

        //private static byte[] CreateHash(string password)
        //{
        //    var hashmd5 = new MD5CryptoServiceProvider();
        //    var pwdhash = hashmd5.ComputeHash(Encoding.Unicode.GetBytes(password));
        //    hashmd5.Clear();

        //    return pwdhash;
        //}
        #endregion
        public static string GenerateUniqueId(int length = 30)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var ticks = DateTime.Now.Ticks.ToString();
            var id = $"{ticks.Substring(ticks.Length - 10)}{guid}";

            return id.Substring(0, length);
        }

        public static string GenerateUniqueFileName(string fileName)
        {
            return fileName.Trim()
                    .ToLower()
                    .Replace(" ", "-")
                    .Replace("/", string.Empty)
                    .Replace("\\", string.Empty)
                    .Replace("&", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("#", string.Empty)
                    .Replace("$", string.Empty)
                    .Replace("*", string.Empty)
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty)
                    .Replace("+", string.Empty)
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Replace("<", string.Empty)
                    .Replace(">", string.Empty)
                    .Replace("'", string.Empty)
                    .Replace("\"", string.Empty)
                    .Replace("_", "-")
                    .Replace(Path.GetExtension(fileName).ToLower(), string.Empty)
                    .SubstringIt(245) +
                "-" +
                GenerateUniqueId(10) +
                Path.GetExtension(fileName).ToLower();
        }

        public static bool IsImage(string fileName)
        {
            return fileName.ContainsIgnoreCase(".jpg") ||
                fileName.ContainsIgnoreCase(".jpeg") ||
                fileName.ContainsIgnoreCase(".png") ||
                fileName.ContainsIgnoreCase(".gif") ||
                fileName.ContainsIgnoreCase(".bmp") ||
                fileName.ContainsIgnoreCase(".ico");
        }

        public static Bitmap ConvertToBitmap(byte[] buffer)
        {
            var ic = new ImageConverter();

            return buffer?.Length > 0 ? ic.ConvertFrom(buffer) as Bitmap : null;
        }

        public static byte[] ConvertToByteArray(Bitmap image)
        {
            var stream = new MemoryStream();
            image?.Save(stream, ImageFormat.Png);
            return stream.GetBuffer();
        }

        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            if (source == null)
            {
                return default;
            }

            IFormatter formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);

                return (T)formatter.Deserialize(stream);
            }
        }

        public static string GetStreamContents(Stream stream)
        {
            var b = new BinaryReader(stream);
            var binData = b.ReadBytes(Convert.ToInt32(stream.Length));

            return Encoding.UTF8.GetString(binData);
        }

        public static DataTable GetDataTableFromCsv(string data)
        {
            var dataTable = new DataTable();
            var lines = data.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
            var columns = lines.First().Split(',').Select(x => x.Replace(" ", string.Empty).Trim());

            foreach (var column in columns)
            {
                dataTable.Columns.Add(new DataColumn(column, typeof(string)) { DefaultValue = string.Empty });
            }

            foreach (var line in lines.Skip(1).Where(x => !EmptyLine(x)))
            {
                try
                {
                    var row = dataTable.NewRow();
                    var regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                    var segments = regex.Split(line).Select(x => x.Trim().Replace("\"", string.Empty));

                    for (var i = 0; i < columns.Count(); i++)
                    {
                        row[i] = segments.ElementAt(i);
                    }

                    dataTable.Rows.Add(row);
                }
                catch (Exception ex)
                {
                    ExceptionLogger.LogException(ex);
                    var s = string.Empty;
                }
            }

            return dataTable;
        }

        public static bool EmptyLine(string line)
        { return line.Split(',').Select(x => x.Trim()).All(value => string.IsNullOrWhiteSpace(value)); }

        public static string GenerateCsv<T>(IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties();
            var csvData = new StringBuilder();

            //Append header row
            csvData.AppendLine(
                string.Join(
                    ",",
                    properties.Where(x => !x.Name.EqualsIgnoreCase("Xml") && x.CanWrite).Select(f => f.Name).ToArray()));

            foreach (var item in data)
            {
                var line = new StringBuilder();

                foreach (var property in properties.Where(x => !x.Name.EqualsIgnoreCase("Xml") && x.CanWrite))
                {
                    if (line.Length > 0)
                    {
                        line.Append(",");
                    }

                    var propertyVal = property.GetValue(item, null);

                    if (propertyVal != null)
                    {
                        var value = $"\"{propertyVal}\"";

                        line.Append(value);
                    }
                    else
                    {
                        line.Append("\"\"");
                    }
                }

                csvData.AppendLine(line.ToString());
            }

            return csvData.ToString();
        }

        public static string DataTableToCsv(DataTable table)
        {
            var result = new StringBuilder();

            for (var i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
            }

            foreach (DataRow row in table.Rows)
            {
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var value = row[i].ToString();

                    if (!string.IsNullOrWhiteSpace(value) && value.StartsWith("<"))
                    {
                        value = Regex.Replace(value, "<[^>]*>", string.Empty);
                    }

                    result.Append($"\"{value.Replace("\"", "\"\"")}\"");
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
                }
            }

            return result.ToString();
        }

        public static string GenerateString(
            Dictionary<string, string> dictionary,
            string entryDelimeter = "__!__",
            string keyValueDelimeter = "__@__")
        {
            var items = dictionary.Select(keyValue => $"{keyValue.Key}{keyValueDelimeter}{keyValue.Value}").ToList();

            return string.Join(entryDelimeter, items);
        }

        public static Dictionary<string, string> GenerateDictionary(
            string value,
            string entryDelimeter = "__!__",
            string keyValueDelimeter = "__@__")
        {
            var dictionary = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(value))
            {
                foreach (var content in Regex.Split(value, entryDelimeter))
                {
                    var segments = Regex.Split(content, keyValueDelimeter);

                    if (segments.Length == 2)
                    {
                        dictionary.Add(segments[0], segments[1]);
                    }
                }
            }

            return dictionary;
        }

        public static Stream SquareImage(Stream stream, int scaleWidth = 200, int scaleHeight = 200)
        {
            var newImage = new WebImage(stream);
            var width = newImage.Width;
            var height = newImage.Height;

            if (width > height)
            {
                var leftRightCrop = (width - height) / 2;
                newImage.Crop(0, leftRightCrop, 0, leftRightCrop);
            }
            else if (height > width)
            {
                var topBottomCrop = (height - width) / 2;
                newImage.Crop(topBottomCrop, 0, topBottomCrop);
            }

            newImage.Resize(scaleWidth, scaleHeight);

            return new MemoryStream(newImage.GetBytes());
        }

        public static Stream SquareAndScaleImage(Stream stream, int scaleWidth = 200, int scaleHeight = 200)
        {
            var newImage = new WebImage(stream);
            var width = newImage.Width;
            var height = newImage.Height;

            if (width > height)
            {
                var leftRightCrop = (width - height) / 2;
                newImage.Crop(0, leftRightCrop, 0, leftRightCrop);
            }
            else if (height > width)
            {
                var topBottomCrop = (height - width) / 2;
                newImage.Crop(topBottomCrop, 0, topBottomCrop);
            }

            newImage.Resize(scaleWidth, scaleHeight);

            return new MemoryStream(newImage.GetBytes());
        }

        public static string RemoveUrlComponent(string url, string component)
        {
            if (!string.IsNullOrEmpty(url))
            {
                foreach (var seg in url.SplitToList('?').Last().SplitToList('&'))
                {
                    var field = seg.SplitToList('=');

                    if (field.First().Equals(component))
                    {
                        return url.Replace(seg, string.Empty);
                    }
                }
            }

            return url;
        }

        public static string GetUrlComponentValue(string url, string component)
        {
            if (!string.IsNullOrEmpty(url))
            {
                foreach (var seg in url.SplitToList('?').Last().SplitToList('&'))
                {
                    var field = seg.SplitToList('=');

                    if (field.First().Equals(component))
                    {
                        return HttpUtility.UrlDecode(field.Last());
                    }
                }
            }

            return null;
        }

        public static string GenerateVerificationCode()
        {
            var randomNumber = new Random();
            var number = randomNumber.Next(000000, 999999).ToString().GetLastCharacters(6);

            if (Convert.ToInt64(number) < 9999)
            {
                number = string.Concat(number, randomNumber.Next(10, 99).ToString());
            }

            return number;
        }

        public static string GeneratePasswordByNumber(string number)
        {
            number = Regex.Replace(number, @"[^\w\.@-]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1.5))
                .Replace("-", string.Empty);
            var chars = number.ToCharArray();
            var reverse = string.Empty;

            for (var i = chars.Length - 1; i >= 0; i--)
            {
                reverse = string.Concat(reverse, chars[i]);
            }

            return $"#P{reverse}@!";
        }

        public static bool SendMessage(SmsMessage message)
        {
            bool smsSent = false;

            try
            {
                // Check if we have SMS turned on or if we're sending it to the test phone number.
                var smsTurnedOn = "SMSTurnedOn".AppSetting(false);
                var smsTestPhoneNumber = "SMSTestPhoneNumber".AppSetting("(205) 915-7429");
                var phoneNumbers = message.To.Split(',').Select(x => x.Trim()).ToList();

                // Determine if the message should be sent based on the environment and recipient.
                bool shouldSend = smsTurnedOn || phoneNumbers.Contains(smsTestPhoneNumber);

                if (shouldSend)
                {
                    TwilioClient.Init(
                        ApplicationCache.Instance.SmsConfiguration.AccountSid,
                        ApplicationCache.Instance.SmsConfiguration.AuthToken);

                    foreach (var phoneNumber in phoneNumbers)
                    {
                        var msg = MessageResource.Create(
                            to: new PhoneNumber(phoneNumber),
                            @from: new PhoneNumber(ApplicationCache.Instance.SmsConfiguration.FromNumber),
                            body: message.Message);
                        message.Sent = !string.IsNullOrEmpty(msg.Sid);
                        smsSent = message.Sent;
                    }
                }
            }
            catch (ApiException e)
            {
                message.Error = e.Message;
                message.ErrorCode = e.Code.ToString();
                message.Sent = false;
            }

            try
            {
                if (smsSent) // Only save to the database if SMS was successfully sent
                {
                    db.SmsMessages.Add(message);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Handle database save error, log, or rethrow if needed
                var obj = logsRepository.JsonConverter(
                    "Action Name",
                    "SendMessage",
                    "Exception Message",
                    ex.Message,
                    "Inner Exception",
                    ex.InnerException?.ToString(),
                    "Stack Trace",
                    ex.StackTrace,
                    "Source",
                    ex.Source,
                    "Timestamp",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    "SmsMessage Id",
                    message.Id,
                    "Type",
                    message.Type,
                    "To",
                    message.To,
                    "Sent",
                    smsSent);

                const string className = nameof(Utilities);
                var methodName = MethodBase.GetCurrentMethod().Name;
                logsRepository.LogData(methodName, className, message.Type, message.To, LogStatuses.Error, obj);
                message.Sent = false; // Mark message as not sent if there was a database save error
            }

            return smsSent;
        }

        public static bool CreateNotification(Notification notification)
        {
            db.Notifications.Add(notification);
            db.SaveChanges();

            return true;
        }

        public static int GetAccessLevel(string moduleId, IEnumerable<Permissions> permissions)
        {
            //Slim result set to only scan permissions for this module
            var modulePermissions = permissions.Where(
                x => !string.IsNullOrEmpty(x.ModuleId) && x.ModuleId.Equals(moduleId))
                .ToList();

            if (modulePermissions.Any())
            {
                //Grab permission explicitly set for this module
                var userPermission = modulePermissions.FirstOrDefault(
                    x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(Constants.User));

                //Return the maximum operation in remaining permissions
                return userPermission.IsNotNullOrEmpty()
                    ? userPermission.OperationId
                    : modulePermissions.Select(x => x.OperationId).Max(x => x);
            }

            //Default to read write if no permission is defined
            return Operations.NoAccess;
        }

        public static int GetPermissionByPlan(string moduleId, IEnumerable<Permissions> permissions)
        {
            var modulePermissions = permissions.FirstOrDefault(
                x => !string.IsNullOrEmpty(x.ModuleId) && x.ModuleId.Equals(moduleId));

            //Default to NO ACCESS if no permission is defined
            return modulePermissions.IsNotNullOrEmpty() ? modulePermissions.OperationId : Operations.NoAccess;
        }

        public static Permissions GetAccessLevelPermission(string moduleId, IEnumerable<Permissions> permissions)
        {
            //Slim result set to only scan permissions for this module
            var modulePermissions = permissions.Where(
                x => !string.IsNullOrEmpty(x.ModuleId) && x.ModuleId.Equals(moduleId))
                .ToList();

            if (modulePermissions.Any())
            {
                //Grab permission explicitly set for this module
                var userPermission = modulePermissions.FirstOrDefault(
                    x => !string.IsNullOrEmpty(x.Type) && x.Type.Equals(Constants.User));

                if (userPermission != null)
                {
                    return userPermission;
                }

                //Return the maximum operation in remaining permissions
                var max = modulePermissions.Select(x => x.OperationId).Max(x => x);

                return modulePermissions.First(x => x.OperationId.Equals(max));
            }

            //Default to null if nothing is defined
            return null;
        }

        //public static void AutoCreateEvent()
        //{
        //    try
        //    {
        //        DateTime dateLastWeekStart = DateTime.Now.Date.AddDays(-7);
        //        DateTime dateLastWeekEnd = dateLastWeekStart.AddDays(1).AddTicks(-1);
        //        DateTime schedularCreatedOneDayBefore = DateTime.Now.Date.AddDays(-1);

        //        var lastWeekEvents = db.ChurchEvents
        //            .Where(x => x.Frequency == EventFrequency.Weekly
        //                        && x.LastWeeklyEventCreated >= dateLastWeekStart
        //                        && x.LastWeeklyEventCreated <= dateLastWeekEnd)
        //            .ToList();

        //        if (lastWeekEvents.Count > 0)
        //        {
        //            var eventIds = lastWeekEvents.Select(x => x.Id).ToList();
        //            var eventSchedulars = db.ChurchEventScheduler.Where(x => eventIds.Contains(x.EventId)
        //            && (DbFunctions.TruncateTime(x.CreatedDate) >= dateLastWeek) && (DbFunctions.TruncateTime(x.CreatedDate) <= schedularCreatedOneDayBefore)).ToList();

        //            if (eventSchedulars.Count > 0)
        //            {
        //                foreach (var item in eventSchedulars)
        //                {
        //                    ChurchEventScheduler schedular = new ChurchEventScheduler
        //                    {
        //                        CampusId = item.CampusId,
        //                        CreatedBy = item.CreatedBy,
        //                        CreatedDate = DateTime.Now,
        //                        EndTime = item.EndTime,
        //                        EventId = item.EventId,
        //                        HideEventAt = item.HideEventAt,
        //                        Id = Utilities.GenerateUniqueId(),
        //                        ShowEventAt = item.ShowEventAt,
        //                        StartDate = item.StartDate,
        //                        StartTime = item.StartTime
        //                    };
        //                    db.ChurchEventScheduler.Add(schedular);
        //                    db.SaveChanges();
        //                }
        //            }

        //            foreach (var item in lastWeekEvents)
        //            {
        //                item.LastWeeklyEventCreated = DateTime.Now;
        //                db.SaveChanges();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionLogger.LogException(ex);
        //    }
        //}

        public static int CardExpirationCalculateInDays(string month, string year)
        {
            if (year.Trim().Length < 4)
            {
                year = Convert.ToString(
                    CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(year.GetLastCharacters(2))));
            }

            var days = DateTime.DaysInMonth(int.Parse(year), int.Parse(month));
            var iDate = $"{year}-{month}-{days}";
            var startDate = DateTime.Now;
            var endDate = DateTime.Parse(iDate);

            return (endDate.Date - startDate.Date).Days;
        }

        public static int CalculateRemainingInDays(this DateTime date)
        {
            var startDate = DateTime.Now;

            return (date.Date - startDate.Date).Days;
        }

        public static void AddToCookies(string name, DateTime expiry, object obj)
        {
            try
            {
                if (HttpContext.Current.Request.Cookies[name] != null)
                {
                    HttpContext.Current.Response.Cookies[name].Expires = DateTime.Now.AddDays(-1);
                }

                var objectJson = new JavaScriptSerializer().Serialize(obj);
                var cookie = new HttpCookie(name, objectJson) { Expires = expiry };
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }
        }

        public static T ReadCookies<T>(string name) where T : class
        {
            var cookies = HttpContext.Current.Request.Cookies[name];

            if (!cookies.IsNotNull())
            {
                return default;
            }

            var jss = new JavaScriptSerializer();

            return jss.Deserialize<T>(cookies.Value);
        }

        public static bool HasCookies(string name)
        {
            var cookies = HttpContext.Current.Request.Cookies[name];

            return cookies.IsNotNull();
        }

        public static void RemoveCookies(string name)
        {
            try
            {
                if (HttpContext.Current.Request.Cookies[name] != null)
                {
                    HttpContext.Current.Response.Cookies[name].Expires = DateTime.Now.AddDays(-1);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }
        }

        public static string TimeAgoUtil(this DateTime dateTime)
        {
            var result = string.Empty;
            var timeSpan = DateTime.Now.Date.Subtract(dateTime.Date);

            if (dateTime.Date == DateTime.Now.Date)
            {
                result = dateTime.ToString("h:mm tt");
            }
            // else if (dateTime.Year != DateTime.Now.Year)
            else if (timeSpan.Days > 60)
            {
                result = dateTime.ToString("MM/dd/yy");
            }
            // else if (timeSpan.Days > 7)
            else if (timeSpan.Days > 7 && timeSpan.Days <= 60)
            {
                result = dateTime.ToShortDateYearString();
            }
            else if (timeSpan.Days <= 7)
            {
                if (timeSpan.Days == 1)
                {
                    result = $"{timeSpan.Days} day ago";
                }
                else
                {
                    result = $"{timeSpan.Days} days ago";
                }
            }

            //if (timeSpan <= TimeSpan.FromSeconds(60))
            //{
            //    result = string.Format("{0} seconds ago", timeSpan.Seconds);
            //}
            //else if (timeSpan <= TimeSpan.FromMinutes(60))
            //{
            //    result = timeSpan.Minutes > 1 ?
            //        String.Format("{0} minutes ago", timeSpan.Minutes) :
            //        "minute ago";
            //}
            //else if (timeSpan <= TimeSpan.FromHours(24))
            //{
            //    result = timeSpan.Hours > 1 ?
            //        String.Format("{0} hours ago", timeSpan.Hours) :
            //        "an hour ago";
            //}
            //else if (timeSpan <= TimeSpan.FromDays(30))
            //{
            //    result = timeSpan.Days > 1 ?
            //        String.Format("{0} days ago", timeSpan.Days) :
            //        "yesterday";
            //}
            //else if (timeSpan <= TimeSpan.FromDays(365))
            //{
            //    result = timeSpan.Days > 30 ?
            //        String.Format("{0} months ago", timeSpan.Days / 30) :
            //        "a month ago";
            //}
            //else
            //{
            //    result = timeSpan.Days > 365 ?
            //        String.Format("{0} years ago", timeSpan.Days / 365) :
            //        "a year ago";
            //}

            return result;
        }

        public static void SetSessionVariables(ApplicationUser user, string churchId = null)
        {
            SessionVariables.Clear();

            if (user == null)
            {
                return;
            }

            var roles = adoData.ReadRolesByUserId(user.Id);
            var userSettings = adoData.ReadUserSettingsByUserId(user.Id);
            var modules = adoData.ReadPermittedModulesByRolesAndUserId(roles, user.Id);
            var permissions = adoData.ReadPermissionsByModulesAndUserId(modules, roles, user.Id);
            var widgets = GetActiveWidgetSortable(user.Id, roles.ConvertAll(x => x.Id));
            var reportCategories = !string.IsNullOrEmpty(churchId)
                ? GetCurrentReports(user.Id, churchId)
                : new List<ReportCategory>();

            if (userSettings.IsNotNull() && churchId.IsNullOrEmpty())
            {
                if (userSettings.PrimaryChurchId == "unregister")
                {
                    userSettings.PrimaryChurchId = null;
                }

                if (userSettings.PrimaryChurchCampusId == "unregister")
                {
                    userSettings.PrimaryChurchCampusId = null;
                }

                SessionVariables.SetCurrentUser(
                    user,
                    roles,
                    permissions,
                    widgets,
                    userSettings.PrimaryChurchId,
                    userSettings.PrimaryChurchCampusId,
                    reportCategories);

                return;
            }

            SessionVariables.SetCurrentUser(user, roles, permissions, widgets, churchId, null, reportCategories);
        }

        public static List<string> GetDynamicColumns(dynamic obj)
        {
            var result = new List<string>();

            if (obj.Count == 0)
            {
                return result;
            }

            foreach (var item in obj[0])
            {
                result.Add(item.Key);
            }

            return result;
        }

        public static DateRange GetDateRange()
        {
            if (DateTime.Now.Month <= 6)
            {
                return new DateRange
                {
                    StartDate = Convert.ToDateTime($"01/01/{DateTime.Now.Year}"),
                    EndDate = Convert.ToDateTime($"06/30/{DateTime.Now.Year}")
                };
            }

            return new DateRange
            {
                StartDate = Convert.ToDateTime($"06/01/{DateTime.Now.Year}"),
                EndDate = Convert.ToDateTime($"12/31/{DateTime.Now.Year}")
            };
        }

        public static IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            while (startDate <= endDate)
            {
                yield return startDate;
                startDate = startDate.AddDays(1);
            }
        }

        public static List<DateTime> GetDateListByTwoDates(DateTime StartDate, DateTime EndDate)
        {
            if (EndDate < StartDate)
            {
                return new List<DateTime>();
            }

            var days = (int)(EndDate.Date - StartDate.Date).TotalDays + 1;

            return Enumerable.Range(0, days).Select(i => StartDate.AddDays(i).Date).OrderBy(q => q).ToList();
        }

        public static DateRange CalculateDateRange(DateTime? startDate, DateTime? endDate, int years)
        {
            DateRange dateRange = new DateRange();

            if (startDate.HasValue)
            {
                dateRange.StartDate = startDate.Value;
            }
            else
            {
                // If startDate is not provided, calculate it based on the specified number of years
                dateRange.StartDate = DateTime.Now.AddYears(-years);
            }

            if (endDate.HasValue)
            {
                dateRange.EndDate = endDate.Value;
            }
            else
            {
                // If endDate is not provided, it can be set to the current date
                dateRange.EndDate = DateTime.Now;
            }

            return dateRange;
        }

        public static int GetPercent(int value, int total)
        { return value == 0 || total == 0 ? 0 : value * 100 / total; }

        public static string StringCombine(string value1, string value2)
        {
            if (value1.IsNotNullOrEmpty() && value2.IsNotNullOrEmpty())
            {
                return $"{value1} {value2}";
            }

            if (value1.IsNotNullOrEmpty() && value2.IsNullOrEmpty())
            {
                return value1;
            }

            if (value1.IsNullOrEmpty() && value2.IsNotNullOrEmpty())
            {
                return value2;
            }

            return null;
        }

        public static string ValidateEmailStatusUpdater(string emailBody)
        {
            const string url = "{site-url}/emails/updatestatus?id={email-id}";
            var imageTag = $"<img src='{url}' width='0' height='0' />";

            if (!emailBody.Contains(imageTag))
            {
                if (emailBody.Contains("</td>") && emailBody.Contains("</table>"))
                {
                    emailBody = emailBody.Replace(
                        "<td align='center'>",
                        $"<td align='center'><img src='{url}' width='0' height='0' />");
                }
                else
                {
                    emailBody += imageTag;
                }
            }

            return emailBody;
        }

        public static DateRange GetReportDefaultDateRange(string userId, string startDate, string endDate)
        {
            // If both startDate and endDate are provided, parse them and return
            if (startDate.IsNotNullOrEmpty() && endDate.IsNotNullOrEmpty())
            {
                return new DateRange { StartDate = DateTime.Parse(startDate), EndDate = DateTime.Parse(endDate) };
            }

            // Consolidate database query
            var reportSettings = db.ReportSettings.FirstOrDefault(x => x.UserId == userId);

            // Set dates based on database query result
            var defaultStartDate = reportSettings?.DateFrom ?? DateTime.Parse($"01/01/{DateTime.Now.Year}");
            var defaultEndDate = reportSettings?.DateEnd ?? DateTime.Parse($"12/31/{DateTime.Now.Year}");

            return new DateRange { StartDate = defaultStartDate, EndDate = defaultEndDate };
        }

        public static DateRange GetDateRangesOfLastNumberOfWeeks(int count, string firstDay = "")
        {
            // Use GetDatesOfLastNumberOfWeeks to get the date range of the last 'count' number of weeks
            var weekDates = ExtensionMethods.GetDatesOfLastNumberOfWeeks(DateTime.Now.Date, count, true, firstDay);

            var startDate = weekDates.FirstOrDefault()?.FirstOrDefault() ?? DateTime.MinValue;
            var endDate = weekDates.LastOrDefault()?.LastOrDefault() ?? DateTime.MinValue;

            return new DateRange { StartDate = startDate, EndDate = endDate };
        }

        public static string CombineFileName(string value1, string value2, string format)
        { return $"{value1}_{value2}.{format}"; }

        public static bool IsDonorOnly(List<ApplicationRoles> roles)
        { return roles.Count == 1 && roles.Any(x => x.Name.EqualsIgnoreCase(Roles.Donor)); }

        public static List<WidgetSortable> GetActiveWidgetSortable(string userId, List<string> roles)
        {
            //1. Get active dashboard for user or default if none found
            //2. get widgets for dashboard

            var settings = db.UserSettings.FirstOrDefault(x => x.UserId == userId);

            // If user has a specific dashboard template set in settings, retrieve widgets for that template
            if (settings.IsNotNull() && settings.DashboardTemplateId.IsNotNullOrEmpty())
            {
                var widgets = GetManageLayoutDashboard(userId, settings.DashboardTemplateId);

                var result = widgets.MainWidgetSortable;
                result.AddRange(widgets.TileWidgetSortable);

                return result;
            }

            return GetDefaultWidgetSortable(userId, roles);
        }

        public static ManageLayoutVM GetManageLayoutDashboard(string userId, string templateId)
        {
            var mainWidgets = GetDashboardWidgetSortOrder(userId, templateId)
                                .Where(x => x.Widget.Location.Equals(WidgetLocations.Main))
                                .ToList();

            var tileWidgets = GetDashboardWidgetSortOrder(userId, templateId)
                                .Where(x => x.Widget.Location.Equals(WidgetLocations.Top))
                                .ToList();

            return new ManageLayoutVM
            {
                MainWidgetSortable = mainWidgets,
                TileWidgetSortable = tileWidgets,
                DashboardTemplate = db.DashboardTemplate.Find(templateId)
            };
        }

        public static List<WidgetSortable> GetDefaultWidgetSortable(string userId, List<string> roles)
        {
            if (roles == null || roles.Count == 0)
            {
                // Fetch roles if not provided
                roles = adoData.ReadUserRolesByUserId(userId).Select(x => x.RoleId).ToList();
            }

            if (roles.IsNotNull() && roles.Count > 0)
            {
                var templateId = GetDashboardTemplatePermissionsByRoles(roles).Select(x => x.TemplateId).FirstOrDefault();

                if (templateId.IsNotNull())
                {
                    return GetDashboardWidgetSortOrder(userId, templateId);
                }
            }
            return new List<WidgetSortable>();
        }

        public static List<DashboardTemplatePermission> GetDashboardTemplatePermissionsByRoles(List<string> roles)
        {
            List<DashboardTemplatePermission> templatePermissiosns = new List<DashboardTemplatePermission>();

            if (roles.IsNotNull() && roles.Count > 0)
            {
                templatePermissiosns = db.DashboardTemplatePermission.Where(x => roles.Contains(x.RoleId)).ToList();
            }

            return templatePermissiosns;
        }

        public static List<WidgetSortable> GetDashboardWidgetSortOrder(string userId, string templateId)
        {
            var widgets = GetWidgetsByTemplate(templateId);
            var sorting = GetDashboardWidgetSortOrders(userId, templateId);

            var result = new List<WidgetSortable>();

            // Create dictionaries for quick lookup
            var widgetsDict = widgets.ToDictionary(w => w.Id);
            var sortingDict = sorting.GroupBy(s => s.WidgetId)
                                     .Select(g => g.First())
                                     .ToDictionary(s => s.WidgetId);

            // Add sorted widgets
            foreach (var sortOrder in sortingDict.Values)
            {
                if (widgetsDict.TryGetValue(sortOrder.WidgetId, out var widget))
                {
                    result.Add(new WidgetSortable
                    {
                        SortOrder = sortOrder.SortOrder,
                        Widget = widget
                    });
                }
            }

            // Add remaining unsorted widgets
            var currentSortOrder = sorting.Count > 0 ? sorting.Max(s => s.SortOrder) + 1 : 1;
            foreach (var widget in widgets)
            {
                if (!sortingDict.ContainsKey(widget.Id))
                {
                    result.Add(new WidgetSortable
                    {
                        SortOrder = currentSortOrder++,
                        Widget = widget
                    });
                }
            }

            // Sort the final list by SortOrder
            return result.OrderBy(x => x.SortOrder).ToList();
        }

        public static List<Widget> GetWidgetsByTemplate(string templateId, string widgetLocation = null)
        {
            var widgetsQuery = db.Widget
                .Join(db.DashboardWidget,
                      widget => widget.Id,
                      dashboardWidget => dashboardWidget.WidgetId,
                      (widget, dashboardWidget) => new { widget, dashboardWidget })
                .Where(x => x.dashboardWidget.TemplateId == templateId);

            if (!string.IsNullOrEmpty(widgetLocation))
            {
                widgetsQuery = widgetsQuery.Where(x => x.widget.Location == widgetLocation);
            }

            return widgetsQuery.Select(x => x.widget).Distinct().ToList();
        }

        public static List<DashboardWidgetSortOrder> GetDashboardWidgetSortOrders(string userId, string templateId)
        {
            return db.DashboardWidgetSortOrder
                .Where(x => x.UserId == userId && x.TemplateId == templateId)
                .ToList();
        }

        public static List<ReportCategory> GetCurrentReports(string userId, string churchId)
        {
            // Retrieve favorite report IDs for the user
            var favoriteReportIds = db.FavoriteReports
                .Where(fr => fr.UserId == userId)
                .Select(fr => fr.ReportId)
                .ToList();

            // Retrieve reports and their corresponding modules
            var reports = db.Reports.Where(r => r.ChurchId == churchId && r.ReportUrl != null).ToList();

            // Retrieve fixed reports for the church
            reports.AddRange(GetFixedReportsForChurch(churchId));

            // Retrieve modules for the reports
            var modules = db.Modules.ToList();

            // Update reports with favorite status and module ID
            foreach (var report in reports)
            {
                report.Favorite = favoriteReportIds.Contains(report.Id);
                report.ModuleId = modules
                    .FirstOrDefault(m => string.Equals(m.Name, report.Name, StringComparison.OrdinalIgnoreCase))?.Id;
            }

            // Retrieve report categories
            var reportCategories = db.ReportCategories.ToList();

            // Assign reports to their respective categories
            foreach (var reportCategory in reportCategories)
            {
                reportCategory.Reports = reports
                    .Where(r => r.ReportCategoryId == reportCategory.Id)
                    .ToList();
            }

            return reportCategories;
        }

        public static List<Report> GetFixedReportsForChurch(string churchId)
        {
            // Retrieve fixed reports and create Report objects for the specified church
            var fixedReports = GetFixedReports();
            return fixedReports.ConvertAll(
                x => new Report
                {
                    Id = x.Id,
                    ReportCategoryId = x.ReportCategoryId,
                    ChurchId = churchId,
                    Name = x.Name,
                    Description = x.Description,
                    ReportUrl = x.URL,
                    ReportType = ReportTypes.Fixed,
                    ReportTypeId = x.Id,
                    CreatedDate = x.CreatedDate,
                    CreatedBy = x.CreatedBy,
                    ModifiedDate = null,
                    ModifiedBy = null,
                    StartDate = DateTime.Parse($"01/01/{DateTime.Now.Year}"),
                    EndDate = DateTime.Parse($"12/31/{DateTime.Now.Year}"),
                    IsDefaultDateRange = false
                });
        }

        public static List<FixedReport> GetFixedReports() { return db.FixedReports.OrderBy(x => x.Name).ToList(); }

        public static string[] GetMonthForChart()
        {
            if (DateTime.Today.Month <= 6)
            {
                return new[] { "January", "February", "March", "April", "May", "June" };
            }

            return new[] { "July", "August", "September", "October", "November", "December" };
        }

        public static string GetFreeTrialDays()
        {
            const string FreeTrialDaysKey = "FreeTrialDays";
            string freeTrialDays;

            try
            {
                freeTrialDays = "FreeTrialDays".AppSetting("30");
            }
            catch (Exception ex)
            {
                // Handle the error (log it, rethrow it, or use a default value)
                throw new ConfigurationErrorsException($"Error retrieving the {FreeTrialDaysKey} setting.", ex);
            }

            return freeTrialDays;
        }

        public static decimal GetMonthlySubscriptionAmount()
        {
            const string monthlySubscriptionFeeKey = "MonthlySubscriptionFee";
            decimal monthlySubscriptionFee;

            try
            {
                monthlySubscriptionFee = "MonthlySubscriptionFee".AppSetting<decimal>(0m);
            }
            catch (Exception ex)
            {
                // Handle the error (log it, rethrow it, or use a default value)
                throw new ConfigurationErrorsException($"Error retrieving the {monthlySubscriptionFeeKey} setting.", ex);
            }

            return monthlySubscriptionFee;
        }

        public static decimal GetAnnualSubscriptionAmount()
        {
            const string annualSubscriptionFeeKey = "AnnualSubscriptionFee";
            decimal annualSubscriptionFee;

            try
            {
                annualSubscriptionFee = "AnnualSubscriptionFee".AppSetting<decimal>(0m);
            }
            catch (Exception ex)
            {
                // Handle the error (log it, rethrow it, or use a default value)
                throw new ConfigurationErrorsException($"Error retrieving the {annualSubscriptionFeeKey} setting.", ex);
            }

            return CalculateMonthlyBillingAmount(annualSubscriptionFee);
        }

        private static decimal CalculateMonthlyBillingAmount(decimal annualSubscriptionFee)
        {
            return Math.Round(annualSubscriptionFee / 12, 2, MidpointRounding.AwayFromZero);
        }

        public static string GetCardProvider(string cardNumber)
        {
            cardNumber = cardNumber.Replace(" ", string.Empty).Replace("-", string.Empty);

            // Regular expressions for different card providers
            var cardProviders = new Dictionary<string, string>
            {
                { "^4[0-9]{12}(?:[0-9]{3})?$", "Visa" },
                { "^5[1-5][0-9]{14}$", "MasterCard" },
                { "^3[47][0-9]{13}$", "American Express" },
                { "^6(?:011|5[0-9]{2})[0-9]{12}$", "Discover" }
            };

            // Loop through the card providers and check if the card number matches any pattern
            foreach (var provider in cardProviders)
            {
                if (Regex.IsMatch(cardNumber, provider.Key))
                {
                    return provider.Value;
                }
            }

            return "Unknown";
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RegularExpressionIfProvidedAttribute : ValidationAttribute
    {
        private readonly string _pattern;

        public RegularExpressionIfProvidedAttribute(string pattern) { _pattern = pattern; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // No value provided, validation passes
            }

            var regexAttribute = new RegularExpressionAttribute(_pattern);
            if (!regexAttribute.IsValid(value))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class RequiredListAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var list = value as IList;
            return list?.Count > 0;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} field is required.";
        }
    }
}