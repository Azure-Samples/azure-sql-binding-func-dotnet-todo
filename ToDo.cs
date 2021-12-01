// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureSQL.ToDo
{
    public static class ToDo
    {
        // primary endpoint for the ToDo API
        // all other functions are dependencies from this function
        [FunctionName("ToDo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "patch", "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            // verify the id querystring is a Guid type
            string QId = req.Query["id"];
            Guid GId = Guid.Empty;
            if (!Guid.TryParse(QId, out GId) && !string.IsNullOrEmpty(QId))
            {
                return new BadRequestObjectResult($"Invalid id: {QId}");
            }

            // parse the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var bodyObject = JsonConvert.DeserializeObject(requestBody);

            log.LogInformation("ToDo API");
            log.LogInformation("Request Body: " + requestBody);
            log.LogInformation("Method: " + req.Method);

            List<ToDoItem> toDoResponseList = new List<ToDoItem>();
            using (HttpClient client = new HttpClient()) {
                // get uri from app settings
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ToDoUri"));
                // setup the request query parameter for item id
                string queryParams = "?id=" + (QId ?? "");

                // switch based on request method, pass query param and/or body object to the corresponding function
                switch (req.Method)
                {
                    case "GET":
                        var getResponse = await client.GetAsync("GetFunction"+queryParams);

                        // return a list of ToDoItems if no querystring
                        if (string.IsNullOrEmpty(QId))
                        {
                            toDoResponseList = JsonConvert.DeserializeObject<List<ToDoItem>>(await getResponse.Content.ReadAsStringAsync());
                            return new OkObjectResult(toDoResponseList);
                        }
                        // return a single ToDoItem if querystring
                        else 
                        {
                            toDoResponseList = JsonConvert.DeserializeObject<List<ToDoItem>>(await getResponse.Content.ReadAsStringAsync());
                            if (toDoResponseList.Count > 0)
                            {
                                // return the first item in the list (should be only item)
                                return new OkObjectResult(toDoResponseList[0]);
                            } else {
                                // return a 404 for no item found
                                return new NotFoundObjectResult(toDoResponseList);
                            }
                        }
                    case "POST":
                        // create a new ToDoItem from body object
                        var postResponse = await client.PostAsJsonAsync("PostFunction"+queryParams, bodyObject);
                        toDoResponseList = JsonConvert.DeserializeObject<List<ToDoItem>>(await postResponse.Content.ReadAsStringAsync());
                        return new OkObjectResult(toDoResponseList[0]);
                        
                    case "PATCH":
                        // update an item from new data in body object
                        // need both the existing data and the new data to update
                        // create a list and put the existing data in at first position
                        var getExistingResponse = await client.GetAsync("GetFunction"+queryParams);
                        List<ToDoItem> toDoPatchList = JsonConvert.DeserializeObject<List<ToDoItem>>(await getExistingResponse.Content.ReadAsStringAsync());

                        // put the patch body in at second position
                        toDoPatchList.Add(JsonConvert.DeserializeObject<ToDoItem>(requestBody));

                        // call patch endpoint with list of existing and new data
                        var patchResponse = await client.PostAsJsonAsync("PatchFunction"+queryParams, toDoPatchList);
                        toDoResponseList = JsonConvert.DeserializeObject<List<ToDoItem>>(await patchResponse.Content.ReadAsStringAsync());
                        return new OkObjectResult(toDoResponseList[0]);
                        
                    case "DELETE":
                        // delete all items or a specific item from querystring
                        // returns remaining items
                        var deleteResponse = await client.DeleteAsync("DeleteFunction"+queryParams);
                        toDoResponseList = JsonConvert.DeserializeObject<List<ToDoItem>>(await deleteResponse.Content.ReadAsStringAsync());
                        return new OkObjectResult(toDoResponseList);
                    default:
                        //do nothing
                        break;
                }
            }

            // return badrequest
            return new BadRequestObjectResult("Bad Request");
        }
    }
}
