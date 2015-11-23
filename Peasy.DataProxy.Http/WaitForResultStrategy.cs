using System;
using System.Threading.Tasks;

namespace Peasy.DataProxy.Http
{
    public class WaitForResultStrategy : ISynchronousInvocationStrategy
    {
        public void Invoke(Func<Task> func)
        {
            try
            {
                func().Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.GetBaseException();
            }
        }

        public T Invoke<T>(Func<Task<T>> func)
        {
            try
            {
                return func().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.GetBaseException();
            }
        }
    }
}
