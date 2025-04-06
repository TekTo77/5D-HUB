using Confluent.Kafka;
using Newtonsoft.Json;
using Npgsql;
using Order_Service.Core;
using Serilog;
using System.Text.RegularExpressions;
using System.Transactions;
using static Confluent.Kafka.ConfigPropertyNames;
using static Product_Service.SQL.SqlContext;

namespace Order_Service.SQL.Repository
{
    public class Repository
    {
        public static async Task<OrderResponse> GetProductAsync(int id)
        {
            Log.Information($"Начало получения информации о заказе по ID {id}");

            try
            {

                await using (var connection = await SqLcontecst.GetConnectionAsync())
                {
                    const string query = "SELECT o.id AS order_id, json_agg(json_build_object('product_id', p.id, 'product_name', p.name, 'price', p.price, 'quantity', op.quantity))" +
                        " AS product FROM orders o LEFT JOIN orders_product op ON o.id = op.order_id LEFT JOIN product p ON op.product_id = p.id WHERE o.id = @orderId GROUP BY o.id; ";

                    await using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@orderId", id);
                        await using (var reader = await command.ExecuteReaderAsync())
                        {

                            if (await reader.ReadAsync())
                            {
                                
                                var orderId = reader.GetInt32(reader.GetOrdinal("order_id"));
                                var productsJson = reader.IsDBNull(reader.GetOrdinal("product"))
                                                    ? "[]"
                                                    : reader.GetString(reader.GetOrdinal("product"));

                                var products = JsonConvert.DeserializeObject<List<ProductInOrder>>(productsJson);

                               
                                var orderResponse = new OrderResponse
                                {
                                    OrderId = orderId,
                                    Products = products ?? new List<ProductInOrder>()
                                };

                                Log.Information($"Успешно получена информация о заказе по ID {id}");
                                return orderResponse; 
                            }
                            else
                            {
                                Log.Warning($"Заказ с ID {id} не найден");
                                return null; 
                            }

                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при получении информации о заказе по ID {id}  {ex}");
                return null;
               
            }
        }

        private static async Task<int> CreateOrder(int id, NpgsqlTransaction transaction, NpgsqlConnection connection)
        {
            Log.Information("Начало создания закзка");

            try
            {

                const string query = "insert into orders (users_id) values (@users_id) returning id";

                await using (var command = new NpgsqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@users_id", id);
                    var result = await command.ExecuteScalarAsync();

                    int orderid = result == DBNull.Value ? 0 : Convert.ToInt32(result);

                    if (orderid!=0)
                    {
                        Log.Information("Удачное создания заказа");
                        return orderid;
                    }
                    Log.Warning("Не получилось создать заказ");
                    await transaction.RollbackAsync();
                    return 0;

                }
 
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при создании заказа {ex}");
                await transaction.RollbackAsync();
                return 0;
            }
        }



        public static async Task<(bool Flag, int Id)> CreateProductinOrder(CreateOrder order)
        {
            Log.Information($"Начало сохранения продуктов в заказе для пользователя с Id {order.UserID}");
            await using (var connection = await SqLcontecst.GetConnectionAsync())
            {
                await using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {

                        int orderId = await CreateOrder(order.UserID, transaction, connection);
                        if (orderId == 0)
                        {
                            return (false, 0);
                        }
                        const string query = "insert into orders_product (order_id,product_id, quantity) values (@order_id,@product_id, @quantity)";
                        await using (var command = new NpgsqlCommand(query, connection, transaction))
                        {

                            foreach (CreateProductinorder product in order.createProductinorders)
                            {
                                command.Parameters.Clear();

                                command.Parameters.AddWithValue("@order_id", orderId);
                                command.Parameters.AddWithValue("@product_id", product.productid);
                                command.Parameters.AddWithValue("@quantity", product.quantity);

                                int row = await command.ExecuteNonQueryAsync();

                                if (row <= 0)
                                {
                                    await transaction.RollbackAsync();
                                    Log.Warning($"Не получилось сохранить продукт в заказе для пользователя с ID {order.UserID}");
                                    return (false, 0);
                                }
                                
                            }
                            await transaction.CommitAsync();
                            Log.Information($"Удачное сохранение всех продуктов в заказе для пользователя с Id {order.UserID}");
                            return (true, orderId);

                        }



                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Log.Error($"Ошибка при сохранение всех продуктов в заказе для пользователя с Id {order.UserID}" +
                            $"{ex}");
                        return (false, 0);
                    }
                }
            }
        }


    }
}
