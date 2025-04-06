using Newtonsoft.Json;
using Npgsql;
using Serilog;

namespace Product_Service.SQL
{
    public class SqlContext
    {
        public static class SqLcontecst
        {
            private static readonly string ConnectionString;

            static SqLcontecst()
            {
                ConnectionString = LoadConnect();
            }

            public static async Task<NpgsqlConnection> GetConnectionAsync() 
            {
                var connection = new NpgsqlConnection(ConnectionString); 
                try
                {
                    await connection.OpenAsync(); 
                    return connection; 
                }
                catch (Exception ex)
                {
                    Log.Error($"PostgreSQL Exception: {ex.Message}");
                    throw; 
                }
            }

          

            private static string LoadConnect()
            {
                const string configFile = "..\\..\\Config\\DataConfig_stud.json";

                
                if (!File.Exists(configFile))
                {
                    Log.Warning("Конфиг файл был не найден");
                    throw new FileNotFoundException("Конфиг файл был не найден"); 
                }

             
                string jsonContent = File.ReadAllText(configFile);

              
                Config config = JsonConvert.DeserializeObject<Config>(jsonContent);

              
                return $"Host={config.ConnectString.Server};Database={config.ConnectString.DataBase};Username={config.ConnectString.Login};Password={config.ConnectString.Password};";
            }



            private class Config 
            {
                public required ConnectString ConnectString { get; set; } 
            }

            private class ConnectString 
            {
                public required string Server { get; set; } 
                public required string DataBase { get; set; } 
                public required string Login { get; set; } 
                public required string Password { get; set; }
            }
        }
    }
}
