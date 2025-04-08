using Confluent.Kafka;
using Serilog;
using System.Collections.Concurrent;
using System.Globalization;

namespace Order_Service.Servise.Kafka
{

    public class KafkaResponseHandler : IDisposable
    {
        private readonly IConsumer<Null, string> _consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<(int productId, bool isAvailable, string correlationId)>> _pendingRequests;
        private readonly string _responseTopic;
        private bool _isRunning;

        public KafkaResponseHandler(IConfiguration configuration)
        {
            Log.Information("Создание KafkaResponseHandler...");
            _pendingRequests = new ConcurrentDictionary<string, TaskCompletionSource<(int productId, bool isAvailable, string correlationId)>>();

            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
            _responseTopic = configuration["Kafka:ResponseTopic"] ?? "product-check-response";

            Log.Information($"BootstrapServers: {bootstrapServers}");
            Log.Information($"ResponseTopic: {_responseTopic}");

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = $"order-service-response-group-{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Latest 
            };

            _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();

            bool subscribed = false;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    _consumer.Subscribe(_responseTopic);
                    Log.Information("KafkaResponseHandler создан и подписан на топик: " + _responseTopic);
                    subscribed = true;
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error($"Ошибка при подписке на топик {_responseTopic}: {ex.Message}. Попытка {i + 1}/5");
                    Task.Delay(2000).Wait();
                }
            }

            if (!subscribed)
            {
                Log.Fatal($"Не удалось подписаться на топик {_responseTopic} после 5 попыток");
                throw new InvalidOperationException($"Не удалось подписаться на топик {_responseTopic}");
            }

            _isRunning = true;
            Task.Run(() => StartConsuming());
        }

        public async Task<(int productId, bool isAvailable, string correlationId)> AddPendingRequest(string correlationId)
        {
            var tcs = new TaskCompletionSource<(int productId, bool isAvailable, string correlationId)>();
            _pendingRequests.TryAdd(correlationId, tcs);
            Log.Information($"Ожидание ответа для correlationId: {correlationId}");

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                cts.Token.Register(() => tcs.TrySetCanceled());
                try
                {
                    return await tcs.Task;
                }
                catch (TaskCanceledException)
                {
                    Log.Error($"Тайм-аут ожидания ответа для correlationId: {correlationId}");
                    _pendingRequests.TryRemove(correlationId, out _);
                    throw new TimeoutException($"Не удалось получить ответ для correlationId: {correlationId} в течение 10 секунд");
                }
            }
        }

        private void StartConsuming()
        {
            Log.Information("KafkaResponseHandler начал слушать ответы...");
            while (_isRunning)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (result == null)
                    {
                        Log.Debug("Ожидание ответа...");
                        continue;
                    }

                    Log.Information($"Получен ответ: {result.Message.Value}");
                    var (eventType, timestamp, values) = KafkaMesseg.ParseMessage(result.Message.Value);

                    if (eventType != "ProductCheckResponse")
                    {
                        Log.Information("Ответ пропущен: неподходящий тип события");
                        continue;
                    }

                    if (values.Length < 3)
                    {
                        Log.Error($"Неверный формат ответа: {result.Message.Value}");
                        continue;
                    }

                   
                    Log.Information($"Проверка времени сообщения: timestamp={timestamp}, тип={timestamp.GetType()}");
                    if (DateTime.TryParseExact(timestamp, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var messageTime))
                    {
                        var timeDifference = DateTime.UtcNow - messageTime;
                        if (timeDifference > TimeSpan.FromMinutes(1))
                        {
                            Log.Warning($"Сообщение устарело (время: {timestamp}): {result.Message.Value}");
                            continue;
                        }
                    }
                    else
                    {
                        Log.Warning($"Не удалось распарсить время сообщения: {timestamp}");
                    }

                    int productId;
                    bool isAvailable;
                    string correlationId;

                    try
                    {
                        productId = int.Parse(values[0]);
                        isAvailable = bool.Parse(values[1]);
                        correlationId = values[2];
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Ошибка при парсинге ответа: {result.Message.Value}. Ошибка: {ex.Message}");
                        continue;
                    }

                    if (_pendingRequests.TryRemove(correlationId, out var tcs))
                    {
                        Log.Information($"Обработан ответ: productId={productId}, isAvailable={isAvailable}, correlationId={correlationId}");
                        tcs.SetResult((productId, isAvailable, correlationId));
                    }
                    else
                    {
                        Log.Warning($"Ответ с correlationId={correlationId} не ожидался");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Ошибка в KafkaResponseHandler: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            Log.Information("KafkaResponseHandler освобождает ресурсы...");
            _isRunning = false;
            _consumer.Close();
            _consumer.Dispose();
            Log.Information("KafkaResponseHandler освобождён");
        }
    }

}
