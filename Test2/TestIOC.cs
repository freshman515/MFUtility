namespace Test2;

public interface IRepository
{
    void Save();
}

public class SqlRepository : IRepository
{
    public void Save() => Console.WriteLine("Saved to SQL");
}

public interface IUserService
{
    void DoWork();
}

public class UserService : IUserService
{
    private readonly IRepository _repo;

    public UserService(IRepository repo)
    {
        _repo = repo;
    }

    public void DoWork()
    {
        _repo.Save();
        Console.WriteLine("UserService Work Done.");
    }
}
