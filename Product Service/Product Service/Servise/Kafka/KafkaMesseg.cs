using Serilog;

namespace Product_Service.Servise.Kafka
{
    public class KafkaMesseg
    {
        public static (string eventType, string timestamp, string[] values) ParseMessage(string message)
        {
            Log.Information($"Парсинг сообщения: {message}");
            var parts = message.Split(" | ");
            if (parts.Length < 3)
            {
                Log.Error($"Неверный формат сообщения: {message}");
                throw new FormatException("Сообщение должно содержать как минимум 3 части, разделённые ' | '");
            }

            var eventType = parts[0].Trim();
            var timestamp = parts[1].Trim();
            var values = parts.Skip(2).Select(p => p.Trim()).ToArray();
            Log.Information($"Результат парсинга: eventType={eventType}, timestamp={timestamp}, тип timestamp={timestamp.GetType()}");
            return (eventType, timestamp, values);
        }

        public static string CreateMessage(string eventType, params string[] values)
        {
            var message = $"{eventType} | {DateTime.UtcNow:O} | {string.Join(" | ", values)}";
            Log.Information($"Создано сообщение: {message}");
            return message;
        }
    }
}
