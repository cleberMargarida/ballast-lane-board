using BallastLaneBoard.Application.Abstractions;
using BallastLaneBoard.Domain.Identity;

namespace BallastLaneBoard.Application.Identity;

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> FindBySubjectAsync(string externalSubject, CancellationToken cancellationToken = default);
}
