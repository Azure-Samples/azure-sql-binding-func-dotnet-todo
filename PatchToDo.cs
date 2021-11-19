using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureSQL.ToDo
{
    public static class PatchToDo
    {
        [FunctionName("PatchToDo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            [Sql("dbo.ToDo", ConnectionStringSetting = "SqlConnectionString")] IAsyncCollector<ToDoItem> toDoItems)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            List<ToDoItem> incomingToDoItems = JsonConvert.DeserializeObject<List<ToDoItem>>(requestBody);
            // existing at first position, new at second position

            ToDoItem toDoItem = incomingToDoItems[0];
            ToDoItem newToDoItem = incomingToDoItems[1];

            // compare the two items attributes
            if (newToDoItem.title != null)
            {
                toDoItem.title = newToDoItem.title;
            }
            if (newToDoItem.order != null)
            {
                toDoItem.order = newToDoItem.order;
            }
            if (newToDoItem.completed != null)
            {
                toDoItem.completed = newToDoItem.completed;
            }

            await toDoItems.AddAsync(toDoItem);
            await toDoItems.FlushAsync();

            return new OkObjectResult(toDoItem);

        }
    }
}
