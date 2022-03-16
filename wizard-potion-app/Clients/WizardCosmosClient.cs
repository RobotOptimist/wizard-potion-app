using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using wizard_potion_app.Models;

namespace wizard_potion_app.Clients
{
    public class WizardCosmosClient
    {
        private readonly CosmosClient cosmos;
        private readonly Container container;

        public WizardCosmosClient(CosmosClient cosmos)
        {
            this.cosmos = cosmos;
            container = cosmos.GetContainer("WizardsDb", "WizardsCollection");
        }

        public async Task<HttpStatusCode> CreateWizard(Wizard wizard)
        {
            ItemResponse<Wizard> response;
            try
            {
                response = await container.CreateItemAsync(wizard);
                return response.StatusCode;
            }
            catch (CosmosException ex)
            {
                return ex.StatusCode;
            }
        }

        public async Task<Wizard?> GetWizard(string wizardId)
        {
            try
            {
                var partitionKey = wizardId.Split('~').First();
                var wizard = await container.ReadItemAsync<Wizard>(wizardId, new PartitionKey(partitionKey));
                return wizard.Resource;
            }
            catch (CosmosException)
            {
                return null;
            }
        }
    }
}
