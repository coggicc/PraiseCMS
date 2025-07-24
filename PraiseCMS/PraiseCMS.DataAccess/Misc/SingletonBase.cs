using System;
using System.Reflection;

namespace PraiseCMS.DataAccess.Misc
{
    public class SingletonBase<T> where T : class
    {
        private static readonly T instance = (T)typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null).Invoke(new object[0]);

        static SingletonBase() { }

        public static T Instance => instance;
    }
}