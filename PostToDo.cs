using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureSQL.ToDo
{
    public static class PostToDo
    {
        [FunctionName("PostToDo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            [Sql("dbo.ToDo", ConnectionStringSetting = "SqlConnectionString")] IAsyncCollector<ToDoItem> toDoItems)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ToDoItem toDoItem = JsonConvert.DeserializeObject<ToDoItem>(requestBody);

            // generate a new id for the todo item
            toDoItem.Id = Guid.NewGuid();

            // set Url from env variable ToDoUri
            toDoItem.url = Environment.GetEnvironmentVariable("ToDoUri")+"?id="+toDoItem.Id.ToString();

            // if completed is not provided, default to false
            if (toDoItem.completed == null)
            {
                toDoItem.completed = false;
            }

            await toDoItems.AddAsync(toDoItem);
            await toDoItems.FlushAsync();

            return new OkObjectResult(toDoItem);
        }
    }
}
