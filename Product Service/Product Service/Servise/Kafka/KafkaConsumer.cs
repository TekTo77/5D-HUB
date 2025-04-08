using Confluent.Kafka;
using Newtonsoft.Json;
using Npgsql;
using Product_Service.Core;
using Serilog;
using Servise.Tools;
using System.Collections.Generic;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Product_Service.Servise.Kafka
{
    public class ProductKafkaConsumer : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ProductServise _productService;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ProducerConfig _producerConfig;
        private readonly string _requestTopic;
        private readonly string _responseTopic;

        public ProductKafkaConsumer(IConfiguration config, ProductServise productService)
        {
            Log.Information("Создание ProductKafkaConsumer...");
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));

            _requestTopic = _config["Kafka:ProductCheckRequestTopic"];
            _responseTopic = _config["Kafka:ProductCheckResponseTopic"];
            var bootstrapServers = _config["Kafka:BootstrapServers"];

            Log.Information($"BootstrapServers: {bootstrapServers}");
            Log.Information($"RequestTopic: {_requestTopic}");
            Log.Information($"ResponseTopic: {_responseTopic}");

            if (string.IsNullOrEmpty(bootstrapServers) ||
                string.IsNullOrEmpty(_requestTopic) ||
                string.IsNullOrEmpty(_responseTopic))
            {
                throw new InvalidOperationException("Kafka конфигурации отсутствуют.");
            }

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "product-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };
            Log.Information("Конфигурация ProductKafkaConsumer завершена");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(5000, stoppingToken); 
            Log.Information("ProductKafkaConsumer.ExecuteAsync запущен и слушает топик: " + _requestTopic);

            using var consumer = new ConsumerBuilder<Null, string>(_consumerConfig).Build();
           

            using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
            

            try
            {
                consumer.Subscribe(_requestTopic);
                Log.Information("Успешно подписан на топик: " + _requestTopic);
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка при подписке на топик: {ex.Message}");
                throw;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Log.Information("Ожидание сообщения...");
                    var result = consumer.Consume(stoppingToken);
                    Log.Information($"Получено сообщение: {result.Message.Value}");

                    var (eventType, _, values) = KafkaMesseg.ParseMessage(result.Message.Value);
                    Log.Information($"Тип события: {eventType}");

                    if (eventType != "ProductCheckRequest")
                    {
                        Log.Information("Сообщение пропущено: неподходящий тип события");
                        continue;
                    }

                    int productId = int.Parse(values[0]);
                    int quantity = int.Parse(values[1]);
                    string correlationId = values[2];
                    Log.Information($"Обработка: productId={productId}, quantity={quantity}, correlationId={correlationId}");

                    bool isAvailable = false;
                    try
                    {
                        isAvailable = await _productService.CheckProductAvailability(productId, quantity);
                        if (isAvailable)
                        {
                            isAvailable = await _productService.Reservation(productId, quantity);
                        }
                        Log.Information($"Результат: isAvailable={isAvailable}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Ошибка при проверке/резервировании: {ex.Message}");
                        isAvailable = false;
                    }

                    var response = KafkaMesseg.CreateMessage("ProductCheckResponse",
                        productId.ToString(), isAvailable.ToString(), correlationId);
                    await producer.ProduceAsync(_responseTopic, new Message<Null, string> { Value = response }, stoppingToken);
                    Log.Information($"Отправлен ответ: {response}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Ошибка consumer'а: {ex.Message}");
                }
            }

            Log.Information("ProductKafkaConsumer завершён");
            consumer.Close();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("ProductKafkaConsumer остановлен.");
            await base.StopAsync(cancellationToken);
        }
    }
}





