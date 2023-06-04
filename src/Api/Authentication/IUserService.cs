namespace Api.Authentication
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
    }

    public class UserService : IUserService
    {
        public Task<User> Authenticate(string username, string password)
        {
            if (username != "admin" || password != "123")
            {
                return Task.FromResult<User>(null);
            }

            User user = new()
            {
                Username = username,
                Id = Guid.NewGuid().ToString("N")
            };

            return Task.FromResult(user);

        }
    }
}
