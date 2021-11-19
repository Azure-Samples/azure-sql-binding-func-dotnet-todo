using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AzureSQL.ToDo
{
    public static class DeleteToDo
    {
        [FunctionName("DeleteToDo")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)] HttpRequest req,
            ILogger log,
            [Sql("DeleteToDo", CommandType = System.Data.CommandType.StoredProcedure, 
                Parameters = "@Id={Query.id}", ConnectionStringSetting = "SqlConnectionString")] 
                IEnumerable<ToDoItem> toDoItems)
        {
            var toDoList = new List<ToDoItem>();
            IEnumerator<ToDoItem> enumerator = toDoItems.GetEnumerator();

            while(enumerator.MoveNext())
            {
                toDoList.Add(enumerator.Current);
            }

            return new OkObjectResult(toDoList);
        }
    }
}
