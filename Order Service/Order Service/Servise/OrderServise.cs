using Order_Service.Core;
using Order_Service.SQL.Repository;
using Serilog;

namespace Order_Service.Servise
{
    public class OrderServise
    {

        private readonly KafkaProducer _kafkaProducer;
       

        public OrderServise(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
           
        }


        public async Task<OrderResponse> GetOrderInfo(int id)
        {
            OrderResponse result = await Repository.GetProductAsync(id);
            if (result==null)
            {
                return new OrderResponse()
                {

                };
            }

            return result;
        }

        public async Task<(bool Flag, int Id)> CreateOrder(CreateOrder order)
        {
            
            bool allProductsAvailable = await _kafkaProducer.CheckAndReserveProducts(order.createProductinorders);
            if (!allProductsAvailable)
            {
                Log.Warning("Не удалось создать заказ для пользователя с ID: {UserId} из-за недоступности продуктов", order.UserID);
                return (false, 0);
            }

           
            return await Repository.CreateProductinOrder(order);
        }

    }
}
