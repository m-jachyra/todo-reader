using System;
using System.Net;
using System.Text.Json;

namespace ToDoReader
{
    internal class Program
    {
        private static readonly HttpClient Client = new HttpClient();
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Program started");
            
            var lastEventId = "0";
            const int timeout = 5000;
            var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };
            
            while (true)
            {
                using var response = await Client.GetAsync($"http://localhost:5231/api/todo/feed?lastEventId={lastEventId}&timeout={timeout}");

                if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var cloudEvents = JsonSerializer.Deserialize<List<CloudEvent>>(responseBody, serializerOptions);
                    lastEventId = cloudEvents.Last().Id;
                    
                    Console.WriteLine(responseBody);
                    using (var file = new StreamWriter("ToDoItemsCurrentSnapshot.json", true))
                    {
                        await file.WriteLineAsync(JsonSerializer.Serialize(cloudEvents, serializerOptions));
                    }
                }
            }
        }
        
        private class CloudEvent
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public Uri Source { get; set; }
            public DateTimeOffset Time { get; set; }
            public string DataContentType { get; set; }
            public int Subject { get; set; }
            public ToDoItem Data { get; set; }
        }
        
        private class ToDoItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}