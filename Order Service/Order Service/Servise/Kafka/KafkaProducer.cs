using Confluent.Kafka;
using Order_Service.Core;
using Order_Service.Servise.Kafka;
using Serilog;

namespace Order_Service.Servise
{


    public class KafkaProducer : IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;
        private readonly KafkaResponseHandler _responseHandler;

        public KafkaProducer(string bootstrapServers, string topic, KafkaResponseHandler responseHandler)
        {
            Log.Information("Создание KafkaProducer...");
            Log.Information($"BootstrapServers: {bootstrapServers}");
            Log.Information($"Topic: {topic}");

            _responseHandler = responseHandler ?? throw new ArgumentNullException(nameof(responseHandler));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));

            var config = new ProducerConfig { BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers)) };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            Log.Information("KafkaProducer создан");
        }

        public async Task<bool> CheckAndReserveProducts(List<CreateProductinorder> products)
        {
            Log.Information("Проверка и резервирование продуктов...");
            foreach (var product in products)
            {
                var correlationId = Guid.NewGuid().ToString();
                var requestMessage = KafkaMesseg.CreateMessage("ProductCheckRequest",
                    product.productid.ToString(), product.quantity.ToString(), correlationId);
                Log.Information($"Отправка запроса: {requestMessage}");
                await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = requestMessage });

                var responseTask = _responseHandler.AddPendingRequest(correlationId);
                var (productId, isAvailable, _) = await responseTask;

                if (!isAvailable)
                {
                    Log.Warning("Продукт с ID: {ProductId} недоступен или недостаточно на складе", productId);
                    return false;
                }

                Log.Information("Продукт с ID: {ProductId} доступен и зарезервирован", productId);
            }

            Log.Information("Все продукты доступны и зарезервированы");
            return true;
        }

        public async Task Event(string eventType, string payload)
        {
            try
            {
                string eventMessage = $"{eventType} | {DateTime.UtcNow:O} | {payload}";
                Log.Information($"Отправка события: {eventMessage}");
                await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = eventMessage });
                Log.Information("Событие отправлено");
            }
            catch (Exception ex)
            {
                Log.Warning($"Ошибка при отправке события в Kafka: {eventType} {payload}. Ошибка: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            Log.Information("KafkaProducer освобождает ресурсы...");
            _producer?.Dispose();
            Log.Information("KafkaProducer освобождён");
        }
    }


}
