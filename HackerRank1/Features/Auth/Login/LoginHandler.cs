using System.Threading.Tasks;

namespace LibraryService.WebAPI.Features.Auth.Login
{
    public class LoginHandler
    {
        public async Task<User> AuthenticateAsync(string email, string password)
        {
            if (email == "admin" && password == "1234")
            {
                return new User() { Id = 1, Email = email, Password = password, Role = "admin" };
            }

            return null;
        }
    }
}
