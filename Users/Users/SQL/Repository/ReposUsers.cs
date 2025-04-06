
using Npgsql;
using System.Data;

using SeccuretyRepos;
using Servise.Tools;
using Result;
using Product_Service.SQL;
using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;
using Serilog;



namespace UserRepos
{
    public class RepositoryUsers()
    {
       

       

       

        public static async Task<(bool Flag, int id)> Register(string Name, string Login, string PasswordHash)
        {
            Log.Information($"Начало регестрации пользователя с Email {Login}");
            try
            {

                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "insert into Users (name, email, password) values (@name, @email, @password) returning id";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", Name);
                        command.Parameters.AddWithValue("@password", PasswordHash);
                        command.Parameters.AddWithValue("@email", Login);


                        var result = await command.ExecuteScalarAsync();

                        int id = result == DBNull.Value ? 0 : Convert.ToInt32(result);

                        if (id != 0)
                        {
                            Log.Information($"Удачная регестрация пользователя с Email {Login} и ID {id}");
                            return (true, id);
                        }
                        Log.Warning($"Неполучилось зарегестрировать пользователя с Email {Login}");
                        return (false, 0);



                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка {ex} при регистрации пользователя с Email {Login}");
                return (false, 0);
            }
        }

        public static async Task<(bool flag, int id, string hash)> Authorization(string Login)
        {
            Log.Information($"Автризация для пользователя с Email: {Login}");
            try
            {
                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "Select id, password from Users where Email = @email";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        
                        command.Parameters.AddWithValue("@email", Login);


                        await using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int id = reader.GetInt32(0);
                                string hash = reader.GetString(1);
                                Log.Information($"Удачная автризация у пользователя с Email {Login} и ID {id}");
                                return (true,id,hash);
                            }
                            Log.Warning($"Неудалось авторизовать пользователя с Email {Login}");
                            return (false, 0, string.Empty);
                        }



                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка {ex} при авторизации пользователя с Email {Login}");
                return (false, 0 , string.Empty);
            }
        }

    }


   

}