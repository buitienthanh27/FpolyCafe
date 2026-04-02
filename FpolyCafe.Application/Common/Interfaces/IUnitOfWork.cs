using System;
using System.Threading;
using System.Threading.Tasks;

namespace FpolyCafe.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
