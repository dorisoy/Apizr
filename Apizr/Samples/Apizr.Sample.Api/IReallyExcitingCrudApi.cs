using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Apizr.Caching;
using Apizr.Logging;
using Refit;

namespace Apizr.Sample.Api
{
    [CacheIt, LogIt]
    public interface IReallyExcitingCrudApi<T, in TKey> where T : class
    {
        [Post("")]
        Task<T> Create([Body] T payload, CancellationToken cancellationToken = default);

        [Get("")]
        Task<List<T>> ReadAll(CancellationToken cancellationToken = default);

        [Get("/{key}")]
        Task<T> ReadOne([CacheKey] TKey key, CancellationToken cancellationToken = default);

        [Put("/{key}")]
        Task Update(TKey key, [Body] T payload, CancellationToken cancellationToken = default);

        [Delete("/{key}")]
        Task Delete(TKey key, CancellationToken cancellationToken = default);
    }
}
