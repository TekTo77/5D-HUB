using Microsoft.AspNetCore.Identity;

namespace Users.Servise
{
 
    public class PasswordService
    {
        private readonly PasswordHasher<object> _passwordHasher;

        public PasswordService()
        {
            _passwordHasher = new PasswordHasher<object>();
        }


        private async Task<bool> ValidatePassword(string password)
        {
            bool isValid = password.Length is >= 8 and <= 37 &&
                           password.Any(char.IsUpper) &&
                           password.Any(char.IsDigit) &&
                           password.Any(ch => !char.IsLetterOrDigit(ch));

            return isValid;
        }

        public async Task<string> HashPassword(string password)
        {
            if (!await ValidatePassword(password))
            {
                return string.Empty;
            }

            return _passwordHasher.HashPassword(null, password);
        }

        
        public async Task<bool> VerifyPassword(string password, string hashedPassword)
        {

            if (!await ValidatePassword(password))
            {
                return false;
            }
            return _passwordHasher.VerifyHashedPassword(null, hashedPassword, password) == PasswordVerificationResult.Success;
        }
    }

}
