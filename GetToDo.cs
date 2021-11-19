using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureSQL.ToDo
{
    public static class GetToDos
    {
        [FunctionName("GetToDos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            [Sql("if @Id = '' select Id, [order], title, url, completed from dbo.ToDo else select Id, [order], title, url, completed from dbo.ToDo where @Id = Id", CommandType = System.Data.CommandType.Text, Parameters = "@Id={Query.id}", ConnectionStringSetting = "SqlConnectionString")] IAsyncEnumerable<ToDoItem> toDos)
        {
            string QId = req.Query["id"];

            // if the query string is empty, return all the ToDos
            if (string.IsNullOrEmpty(QId))
            {
                var todoList = new List<ToDoItem>();
                IAsyncEnumerator<ToDoItem> enumerator = toDos.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync())
                {
                    todoList.Add(enumerator.Current);
                }
                await enumerator.DisposeAsync();

                return new OkObjectResult(todoList);
            }
            else
            {
                // if the query string is not empty, return the ToDo with the matching id
                var toDoItem = new ToDoItem();
                IAsyncEnumerator<ToDoItem> enumerator = toDos.GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync())
                {
                    toDoItem = enumerator.Current;
                }
                await enumerator.DisposeAsync();

                return new OkObjectResult(toDoItem);
            }
            
        }
    }
}
