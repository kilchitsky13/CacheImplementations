using System;
using System.Collections.Generic;
using System.Text;

namespace CacheExample.Interfaces
{
    public interface ModelWithId<T>
    {
        T Id { get; set; }
    }
}
