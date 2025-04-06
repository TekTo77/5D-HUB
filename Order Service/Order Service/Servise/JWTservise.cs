using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using Servise.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Product_Service.Servise
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
                Log.Warning("Конфиг файл был не найден");
            }
            string jsonContent = File.ReadAllText(configFile);
            JWTsettings config = JsonConvert.DeserializeObject<JWTsettings>(jsonContent);

            SecretKey = config.SecretKey;
            Audience = config.Audience;

        }



        public static async Task<string> ExtractEmailFromTokenAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");

            return emailClaim?.Value ?? string.Empty;
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


        private class JWTsettings
        {
            public required string SecretKey { get; set; }

            public required string Audience { get; set; }
        }
    }
}
