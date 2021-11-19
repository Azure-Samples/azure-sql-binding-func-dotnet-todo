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
        [FunctionName("ToDo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "patch", "delete", Route = null)] HttpRequest req,
            ILogger log)
        {

            string QId = req.Query["id"];
            string queryParams = "?id=" + (QId ?? "");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation("ToDo API");
            log.LogInformation("Request Body: " + requestBody);
            log.LogInformation("Method: " + req.Method);

            var bodyObject = JsonConvert.DeserializeObject(requestBody);

            List<ToDoItem> toDoResponseList = new List<ToDoItem>();
            ToDoItem toDoResponse = new ToDoItem();
            using (HttpClient client = new HttpClient()) {
                // get uri from app settings
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ToDoUri"));

                // switch based on request method
                switch (req.Method)
                {
                    case "GET":
                        var getResponse = await client.GetAsync("GetToDos"+queryParams);

                        // return a list of ToDoItems if no querystring
                        if (string.IsNullOrEmpty(QId))
                        {
                            toDoResponseList = JsonConvert.DeserializeObject<List<ToDoItem>>(await getResponse.Content.ReadAsStringAsync());
                            return new OkObjectResult(toDoResponseList);
                        } else {
                            // return a single ToDoItem if querystring
                            toDoResponse = JsonConvert.DeserializeObject<ToDoItem>(await getResponse.Content.ReadAsStringAsync());
                            return new OkObjectResult(toDoResponse);
                        }
                    case "POST":
                        var postResponse = await client.PostAsJsonAsync("PostToDo"+queryParams, bodyObject);
                        toDoResponse = JsonConvert.DeserializeObject<ToDoItem>(await postResponse.Content.ReadAsStringAsync());
                        return new OkObjectResult(toDoResponse);
                        
                    case "PATCH":
                        // create a list and put the existing data in at first position
                        List<ToDoItem> toDoPatchList = new List<ToDoItem>();
                        var getExistingResponse = await client.GetAsync("GetToDos"+queryParams);
                        toDoResponse = JsonConvert.DeserializeObject<ToDoItem>(await getExistingResponse.Content.ReadAsStringAsync());
                        toDoPatchList.Add(toDoResponse);

                        // put the patch body in at second position
                        toDoPatchList.Add(JsonConvert.DeserializeObject<ToDoItem>(requestBody));

                        // call patch endpoint with list of existing and new data
                        var patchResponse = await client.PostAsJsonAsync("PatchToDo"+queryParams, toDoPatchList);
                        toDoResponse = JsonConvert.DeserializeObject<ToDoItem>(await patchResponse.Content.ReadAsStringAsync());
                        return new OkObjectResult(toDoResponse);
                        
                    case "DELETE":
                        var deleteResponse = await client.DeleteAsync("DeleteToDo"+queryParams);
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
