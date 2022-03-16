using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using wizard_potion_app.Clients;
using wizard_potion_app.Models;
using System.Net;

namespace wizard_potion_app
{
    public class WizardActions
    {
        private readonly WizardCosmosClient cosmosClient;

        public WizardActions(WizardCosmosClient cosmosClient)
        {
            this.cosmosClient = cosmosClient;
        }

        [FunctionName("CreateWizard")]
        public async Task<IActionResult> CreateWizard(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Wizard wizard = JsonConvert.DeserializeObject<Wizard>(requestBody);
                if (wizard == null || string.IsNullOrEmpty(wizard.Name) || string.IsNullOrEmpty(wizard.User))
                    return new BadRequestObjectResult("Invalid wizard");

                wizard.Id = $"{wizard.User}~{wizard.Name}";
                var response = await cosmosClient.CreateWizard(wizard);
                if (response == HttpStatusCode.Conflict)
                    return new ConflictObjectResult("Failed to create wizard, a wizard with this name already exists");                

                return new CreatedResult("", wizard);
            }
            catch (Exception ex)
            {
                log.LogError("An error occurred while processing the request", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [FunctionName("GetWizard")]
        public async Task<IActionResult> GetWizard(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
