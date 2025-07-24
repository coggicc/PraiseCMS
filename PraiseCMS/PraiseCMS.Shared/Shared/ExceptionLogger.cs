using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Net.Http;

namespace PraiseCMS.Shared.Shared
{
    public static class ExceptionLogger
    {
        public static void LogException(Exception ex)
        {
            // Log the exception details here            
            System.Diagnostics.Debug.WriteLine($"Exception occurred: {ex.Message}");
            //Console.WriteLine($"Exception occurred: {ex.Message}");
        }

        public static void LogDbUpdateException(DbUpdateException ex)
        {
            // Log DbUpdateException details here            
            System.Diagnostics.Debug.WriteLine($"DbUpdateException occurred: {ex.Message}");
            //Console.WriteLine($"DbUpdateException occurred: {ex.Message}");
        }

        public static void LogSqlException(SqlException ex)
        {
            // Log SqlException details here            
            System.Diagnostics.Debug.WriteLine($"SqlException occurred: {ex.Message}");
            //Console.WriteLine($"SqlException occurred: {ex.Message}");
        }

        public static void LogHttpRequestException(HttpRequestException ex)
        {
            // Log HttpRequestException details here            
            System.Diagnostics.Debug.WriteLine($"HttpRequestException occurred: {ex.Message}");
            //Console.WriteLine($"HttpRequestException occurred: {ex.Message}");
        }
    }
}