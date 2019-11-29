using System;
using CacheExample.Interfaces;

namespace CacheExample.Models
{
    public class CacheResult : ICacheResult
    {
        public CacheResult()
        {
            IsSuccess = true;
            ErrorMessage = string.Empty;
        }

        public CacheResult(Exception ex)
        {
            IsSuccess = false;
            ErrorMessage = ex.Message;
        }

        public CacheResult(string message)
        {
            IsSuccess = false;
            ErrorMessage = message;
        }


        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

    }


    public class CacheResult<T> : CacheResult, ICacheResult<T> 
    {
        public CacheResult(): base()
        {
            Data = (T)GetDefaultValue(typeof(T));
        }

        public CacheResult(Exception ex) : base(ex)
        {
            Data = (T)GetDefaultValue(typeof(T));
        }

        public CacheResult(string message) : base(message)
        {
            Data = (T)GetDefaultValue(typeof(T));
        }

        public CacheResult(T value) : base()
        {
            Data = value;
        }

        public T Data { get; set; }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }

    }
}
