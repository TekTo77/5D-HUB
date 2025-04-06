using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Servise.Tools;
using Newtonsoft.Json;

using Serilog;
using Product_Service.SQL;

namespace SeccuretyRepos
{
    
    class CheckEmail
    {





        public static async Task<bool> ValidateEmailAsync(string email)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            });
        }


        public static async Task<string> GetEmailByIdAsync(int id)
        {
            Log.Information($"Начало получения Email по ID {id}");
            try
            {
                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "select email from Users where id = @id";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        var result = await command.ExecuteScalarAsync();

                        string Email = result == DBNull.Value ? string.Empty : Convert.ToString(result) ?? string.Empty;

                        if (Email.IsNullOrEmpty())
                        {
                            Log.Warning($"Не найден Email по ID {id}");
                            return Email;
                        }
                        Log.Warning($"Получилось получить Email по ID {id}");
                        return Email;

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при получении получить Email по ID {id}");
                return string.Empty;
            }
        }
        public static async Task<int> GetIdlByEmailAsync(string Email)
        {

            Log.Information($"Начало получения ID по Email {Email}");
            try
            {
                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "select id from Users where Email = @email";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {

                        command.Parameters.AddWithValue("@email", Email);

                        var result = await command.ExecuteScalarAsync();

                        int id = result == DBNull.Value ? 0 : Convert.ToInt32(result);
                        if (id != 0)
                        {
                            Log.Information($"Успешное получение ID {id} по Emal {Email}");
                            return id;
                        }
                        Log.Warning($"Не найден ID по Email {Email}");
                        return 0;

                    }
                }
            }
            catch
            {
                Log.Error($"Ошибка при аолучении ID по Email {Email}");
                return 0;
            }
        }

    }
}
