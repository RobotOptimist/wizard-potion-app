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
using System.Security.Claims;

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
            [HttpTrigger("post", Route = null)] HttpRequest req,
            ILogger log,
            ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Wizard wizard = JsonConvert.DeserializeObject<Wizard>(requestBody);
                if (wizard == null || string.IsNullOrEmpty(wizard.Name) || string.IsNullOrEmpty(wizard.User))
                    return new BadRequestObjectResult("Invalid wizard");

                wizard.Id = $"{wizard.User}~{wizard.Name}";
                var response = await cosmosClient.CreateWizard(wizard);
                log.LogInformation("responseStatusCode", response);
                if (response == HttpStatusCode.Conflict)
                    return new ConflictObjectResult("Failed to create wizard, a wizard with this name already exists");                

                return new CreatedResult("", claimsPrincipal?.Identity?.Name);
            }
            catch (Exception ex)
            {
                log.LogError("An error occurred while processing the request", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [FunctionName("GetWizard")]
        public async Task<IActionResult> GetWizard(
            [HttpTrigger("get", Route = "wizards/{id}")] HttpRequest req,
            string id,
            ILogger log, 
            ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var wizard = await cosmosClient.GetWizard(id);           
                if (wizard is null)
                {
                    return new NotFoundObjectResult($"No wizard found matching id: {id}");
                }
                return new OkObjectResult(new { wizard, claimsPrincipal?.Identity?.Name });
            }
            catch (Exception ex)
            {
                log.LogError("Failed to retrieve the wizard due to an application error", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
