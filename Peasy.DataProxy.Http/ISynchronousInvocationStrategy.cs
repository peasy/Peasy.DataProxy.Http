using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peasy.DataProxy.Http
{
    public interface ISynchronousInvocationStrategy
    {
        TOut Invoke<TOut>(Func<Task<TOut>> func);
        void Invoke(Func<Task> func);
    }
}
