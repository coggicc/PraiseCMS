using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Services
{
    public class Result<T>
    {
        public ResultType ResultType { get; set; }
        public ResultIcon ResultIcon { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<T> List { get; set; }
        public Exception Exception;

        public static Result<T> FromAction(T data)
        {
            return new Result<T>
            {
                Data = data,
                ResultType = ResultType.Success
            };
        }

        public static Result<T> FromException(Exception exception, string message, T data)
        {
            return new Result<T>
            {
                Data = data,
                Exception = exception,
                Message = message,
                ResultType = ResultType.Exception
            };
        }

        public static Result<T> Failure(string message)
        {
            return new Result<T>
            {
                Message = message,
                ResultType = ResultType.Failure
            };
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>
            {
                Data = data,
                ResultType = ResultType.Success
            };
        }
    }
}