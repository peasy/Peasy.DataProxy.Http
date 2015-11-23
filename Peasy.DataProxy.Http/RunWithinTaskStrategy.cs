using System;
using System.Threading.Tasks;

namespace Peasy.DataProxy.Http
{
    public class RunWithinTaskStrategy : ISynchronousInvocationStrategy
    {
        public void Invoke(Func<Task> func)
        {
            try
            {
                Task.Run(() => func()).Wait(); 
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
                return Task.Run(() => func()).Result; 
            }
            catch (AggregateException ex)
            {
                throw ex.GetBaseException();
            }
        }
    }
}