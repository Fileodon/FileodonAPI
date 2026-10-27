using Domain.Interface;

namespace Infrastructure.Interface.Repository;

public interface IBaseRepository<T> : IBaseInterface<T> where T : class { }