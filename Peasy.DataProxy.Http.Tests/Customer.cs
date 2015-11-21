using Peasy.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peasy.DataProxy.Http.Tests
{
    public class Customer : IDomainObject<long>
    {
        public long ID { get; set; }
    }
}
