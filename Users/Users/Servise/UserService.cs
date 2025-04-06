
using Result;
using UserRequestese;
using UserRepos;
using SeccuretyRepos;
using Microsoft.IdentityModel.Tokens;
using Users.Servise;
using Newtonsoft.Json.Linq;


namespace UserServise
{


    public class UserService
    {
        private readonly PasswordService _passwordService = new PasswordService();



        public async Task<RegistrResult> RegisterUserAsync(RegisterRequest request)
        {
            if (!await CheckEmail.ValidateEmailAsync(request.Login))
            {
                return new RegistrResult()
                {
                    Flag = false,
                    Userid = 0,
                    Token = string.Empty,
                };
            }

            string hash = await _passwordService.HashPassword(request.Password);




            var result = await RepositoryUsers.Register(request.Name, request.Login, hash);

            string message = result.Flag ? "Регистрация успешна" : "Ошибка регестрации";
           

            return new RegistrResult
            {

                Flag = result.Flag,
                Userid = result.Flag ? result.id : 0,
                Token = await JWTservise.GenerateJwtTokenAsync(result.Flag ? request.Login : string.Empty )
            };
        }

        public async Task<RegistrResult> AuthenticateUserAsync(LoginRequest request)
        {

            if (!await CheckEmail.ValidateEmailAsync(request.Login))
            {
                return new RegistrResult()
                {
                    Flag = false,
                    Userid = 0,
                    Token = string.Empty,
                };
            }

            var result = await RepositoryUsers.Authorization(request.Login);
            if (!await _passwordService.VerifyPassword(request.Password, result.hash) || !result.flag)
            {
                return new RegistrResult()
                {
                    Flag = false,
                    Userid = 0,
                    Token = string.Empty,
                };
            }
            return new RegistrResult()
            {
                Flag = result.flag,
                Userid = result.id,
                Token = await JWTservise.GenerateJwtTokenAsync(request.Login),
            };

        }

             

        


       
        

    }
}
