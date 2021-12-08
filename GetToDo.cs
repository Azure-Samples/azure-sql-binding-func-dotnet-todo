// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

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
        // return items from ToDo table
        // id querystring in the query text to filter if specified
        // uses input binding to run the query and return the results
        [FunctionName("GetToDos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetFunction")] HttpRequest req,
            ILogger log,
            [Sql("if @Id = '' select Id, [order], title, url, completed from dbo.ToDo else select Id, [order], title, url, completed from dbo.ToDo where @Id = Id", CommandType = System.Data.CommandType.Text, Parameters = "@Id={Query.id}", ConnectionStringSetting = "SqlConnectionString")] IAsyncEnumerable<ToDoItem> toDos)
        {
            return new OkObjectResult(toDos);
        }
    }
}
