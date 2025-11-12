using System.Security.Claims;

//pretty sure we can delete this now


namespace WebServerMVCv2.Models
{
    public class UserManager
    {
        //by static it simulates a start up. This is where I'd get user data from 
        public static List<UserAccount> _accounts;

        static UserManager()
        {
            _accounts = 
            [
            new UserAccount
            {
                UserName = "admin",
                Password = "testing123",
                Claims = new List<Claim>
                { 
                    //claim == key value pair
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim("admin", "true")
                }
            },

            new UserAccount
            {
                UserName = "jdoe",
                Password = "testing123",
                Claims = new List<Claim>
                { 
                    //claim == key value pair
                    new Claim(ClaimTypes.Name, "jdoe"),

                }

            }
            ];

        }

        public static UserAccount Login(string username, string password) 
        { 
            return _accounts.FirstOrDefault(acc => acc.UserName == username && acc.Password == password);
        }
    }

    public class UserAccount 
    { 
        public string UserName { get; set; }
        public string Password { get; set; }    

        //claims
        public List<Claim> Claims { get; set; }
    }
}
