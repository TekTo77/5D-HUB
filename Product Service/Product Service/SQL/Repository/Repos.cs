using Npgsql;
using Product_Service.Core;
using Serilog;
using static Product_Service.SQL.SqlContext;

namespace Product_Service.SQL.Repository
{
    public class Repos
    {
        public static async Task<ProductDB> GetProductAsync(int id)
        {
            Log.Information($"Начало получения информации о продукте по ID {id}");

            try
            {

                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "Select id, Name, discription, Price, quantity from Product where @id=id";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        await using (var reader = await command.ExecuteReaderAsync())
                        {
                            
                           if (await reader.ReadAsync())
                           {
                                Log.Information($"Успешное получение информации о продукте по ID {id}");
                                return new ProductDB()
                                {
                                   Id = reader.GetInt32(0),
                                   Name = reader.GetString(1),
                                   Description = reader.GetString(2),
                                   Price= reader.GetDecimal(3),
                                   quantity = reader.GetInt32(4),

                                };
                           }
                            Log.Warning($"Продукт с ID {id} не найден");
                            return new ProductDB()
                            {
                                Name= string.Empty

                            };
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при получении информации о продукте по ID {id}  {ex}");

                return new ProductDB()
                {
                    Name = string.Empty

                }; ;
            }
        }

        public static async Task<bool> CreateProduct(Product product)
        {
            Log.Information("Начало создания продукта");

            try
            {

                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "insert into Product ( name, discription, price, quantity) VALUES ( @name, @discription, @price, @quantity);";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", product.Name);
                        command.Parameters.AddWithValue("@discription", product.Description);
                        command.Parameters.AddWithValue("@price", product.Price);
                        command.Parameters.AddWithValue("@quantity", product.quantity);

                        bool result = await command.ExecuteNonQueryAsync() > 0;

                        if (result)
                        {
                            Log.Information("Удачное создания продукта");
                            return true;
                        }
                        Log.Warning("Не получилось создать продукт");
                        return false;
                        
                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при создании продукта {ex}");
                return false;
            }
        }

        public static async Task<bool> changeQuantity(int id, int quantity)
        {

            Log.Information($"Начало измененя количества продукта на складе с ID {id}");
            try
            {
                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "Update Product set quantity = quantity + @quantity where id = @id";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@quantity", id);
                        command.Parameters.AddWithValue("@id", id);

                        bool result = await command.ExecuteNonQueryAsync() > 0;

                        if (result)
                        {
                            Log.Information($"Удачное изменение количества продукта с ID {id}");
                            return true;
                        }
                        Log.Warning($"Не получилось изменить количества продукта на складе с ID {id}");
                        return false;

                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка  при изменении количества продукта  {ex}");
                return false;
            }
        }

        public static async Task<bool> Reservation(int id, int quantyti)
        {
            try
            {
                Log.Information($"Начало резервации продукта на складе с ID {id}");
                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "Update Product set quantity = quantity - @quantity where id = @id AND (quantity - @quantity) >= 0";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@quantity", quantyti);
                        command.Parameters.AddWithValue("@id", id);

                        bool result = await command.ExecuteNonQueryAsync() > 0;

                        if (result)
                        {
                            
                            Log.Information($"Удачная резервация продукта с ID {id}");
                            return true;

                        }
                        
                        Log.Warning($"Не получилось зарезервирновать продукт на складе с ID {id}");
                        return false;
                        
                    }

                }
            }
            catch(Exception ex)
            {
                Log.Error($"Ошибка  при резервации продукта {ex}");
                return false;
            }
        }

        public static async Task<int> CheckQuntuty(int id)
        {

            Log.Information($"Начало проверки количества товара с Id {id}");
            try
            {
                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "Select quantity from Product where id=@id";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        var result = await command.ExecuteScalarAsync();

                        int resultQuntity = result == DBNull.Value ? 0 : Convert.ToInt32(result);

                        Log.Information($"Удачная проверка количества товара {resultQuntity} с Id {id}");
                        return resultQuntity;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка {ex} при проверки количества товара с ID {id}");
                return 0;
            }
        }

    }
}
