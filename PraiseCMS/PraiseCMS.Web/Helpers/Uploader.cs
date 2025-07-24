using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Web;

namespace PraiseCMS.Web.Helpers
{
    public static class Uploader
    {
        public static bool UploadImage(string fileName, HttpPostedFileBase image)
        {
            try
            {
                string path = $"{AppDomain.CurrentDomain.BaseDirectory}{"Upload.Images".AppSetting("\\Uploads")}\\{fileName}";
                image.SaveAs(path);
                image.InputStream.Close();

                return true;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return false;
            }
        }

        public static bool DeleteImage(string fileName)
        {
            try
            {
                string path = $"{AppDomain.CurrentDomain.BaseDirectory}{"Upload.Images".AppSetting("\\Uploads")}\\{fileName}";

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                return true;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return false;
            }
        }

        public static bool UploadFile(string fileName, HttpPostedFileBase file)
        {
            try
            {
                string path = $"{AppDomain.CurrentDomain.BaseDirectory}{"Upload.Files".AppSetting("\\Uploads")}\\{fileName}";
                file.SaveAs(path);
                file.InputStream.Close();

                return true;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return false;
            }
        }
    }
}