namespace Product_Service.Servise.Kafka
{
    public class KafkaMesseg
    {
        public static string CreateMessage(string eventType, params string[] values)
        {

            string payload = string.Join(" | ", values);

            return $"{eventType} | {DateTime.UtcNow:O} | {payload}";
        }


        public static (string EventType, DateTime Timestamp, string[] Values) ParseMessage(string message)
        {

            string[] parts = message.Split(" | ");
            string eventType = parts[0];
            DateTime timestamp = DateTime.Parse(parts[1]);

            string[] values = parts.Skip(2).ToArray();
            return (eventType, timestamp, values);
        }
    }
}
