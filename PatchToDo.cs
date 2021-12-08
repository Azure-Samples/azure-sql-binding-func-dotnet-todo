// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

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
        // update an item from new data in body object
        // receives a list in the body with the existing data in at first position, and updates in at second position
        // uses output binding to update the row in ToDo table
        [FunctionName("PatchToDo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "PatchFunction")] HttpRequest req,
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
