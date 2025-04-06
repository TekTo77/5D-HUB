

using Result;
using Interfase;

namespace UserRequestese
{


    public class LoginRequest
    {
        public required string Login { get; set; }
        public required string Password { get; set; }


    }

    public class RegisterRequest : LoginRequest
    {
        public required string Name { get; set; }

    }


    


}
 
