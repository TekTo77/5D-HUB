using Microsoft.AspNetCore.Mvc;
using UserRequestese;
using UserServise;
using Result;
using SeccuretyRepos;
using Servise.Tools;
using System.Security.Principal;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics.Eventing.Reader;



namespace UsersControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {

         

        private readonly UserService _userService;

       

      



        public UsersController(UserService userService)
        {
            _userService = userService;
            
        }


       

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {

            if (request == null || request.Password.IsNullOrEmpty() || request.Name.IsNullOrEmpty() || request.Login.IsNullOrEmpty())
            {
                return BadRequest("ошибка данных один из полей равенн пустой строке");
            }
           
            
            RegistrResult result = await _userService.RegisterUserAsync(request);


            if (result.Flag || !result.Token.IsNullOrEmpty())
            {
                return StatusCode(201, new {  result.Token,result.Userid, Success = result.Flag});
            }
            
            return BadRequest("Ошибка ваш пароль слабый или произошла ошибка");
            
            
        }


      
        

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            if (request == null || request.Password.IsNullOrEmpty() || request.Login.IsNullOrEmpty())
            {
                return BadRequest("Ошибка данных один из полей равен пустой строке");
            }

            RegistrResult result = await _userService.AuthenticateUserAsync(request);


            if (result.Flag || !result.Token.IsNullOrEmpty())
            {
                return StatusCode(201, new { result.Token, result.Userid, Success = result.Flag });
            }
            return BadRequest(new { messeg = "Ошибка при авторизации", result.Flag });
            
        }




    }

   
}

