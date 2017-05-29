using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Helper
{

    public interface IDecorator
    {
        Task Decorate(Action action);
        Task<T> Decorate<T>(Func<T> func);
    }

    public static class DecoratorFactory
    {
        public static IDecorator CreateDecorator()
        {
            return new Decorator();
        }
    }
    [Serializable]
    public class Decorator : MarshalByRefObject, Performance.Logger.API.Helper.IDecorator
    {
        private static readonly NLog.ILogger _logger;
        private static readonly string currentAssemblyName = Assembly.GetExecutingAssembly().FullName;
        static Decorator()
        {
            _logger = NLog.LogManager.GetLogger(currentAssemblyName);
        }
        public Task Decorate(Action action)
        {
            return Task.Run(() =>
            {

                try
                {
                    //before execution
                    action();
                    //after execution
                }
                catch (Exception ex)
                {
                    //log exception
                    _logger.Error(ex, ex.Message);
                }
            });
        }
        public Task<T> Decorate<T>(Func<T> func)
        {
            return Task.Run<T>(() =>
            {
                try
                {
                    //before execution
                    return func();
                    //after execution
                }
                catch (Exception ex)
                {
                    //log exception
                    _logger.Error(ex, ex.Message);
                    return default(T);
                }
            });
        }
    }
}
