using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Servise.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Users.Servise
{
    public class JWTservise
    {



        private const string configFile = "..\\..\\Config\\JWTsetings.json";



        private static readonly string SecretKey;
        private static readonly string Audience;

        static JWTservise()
        {
            if (!File.Exists(configFile))
            {
                Console.WriteLine("Конфиг файл был не найден"); // Если нет, выбрасываем исключение
            }
            string jsonContent = File.ReadAllText(configFile);
            JWTsettings config = JsonConvert.DeserializeObject<JWTsettings>(jsonContent);

            SecretKey = config.SecretKey;
            Audience = config.Audience;

        }

        public static async Task<string> GenerateJwtTokenAsync(string userLogin, int expirationMinutes = 30)
        {



            if (userLogin.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: userLogin,
                audience: Audience,
                claims: [new Claim(ClaimTypes.Name, userLogin)],
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes), // ВРЕМЯ ЖИЗНИ ТОКЕНА
                signingCredentials: credentials
            );

            var generatedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return generatedToken;
        }

        public static async Task<bool> ValidateJwtTokenAsync(string token, string email)
        {

            if (email.IsNullOrEmpty())
            {


                return false;
            }


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = email,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = securityKey,
                ValidateIssuerSigningKey = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out _);
                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // public static string GenerateRefreshTokenAsync(string userLogin)

        private class JWTsettings
        {
            public required string SecretKey { get; set; }

            public required string Audience { get; set; }
        }
    }

}
