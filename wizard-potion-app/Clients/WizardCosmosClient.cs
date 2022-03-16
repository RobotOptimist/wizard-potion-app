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

        public WizardCosmosClient(CosmosClient cosmos)
        {
            this.cosmos = cosmos;
        }

        public async Task<HttpStatusCode> CreateWizard(Wizard wizard)
        {
            ItemResponse<Wizard> response;
            try
            {
                var container = cosmos.GetContainer("WizardsDb","WizardCollection");
                response = await container.CreateItemAsync(wizard);
                return response.StatusCode;
            }
            catch (CosmosException ex)
            {
                return ex.StatusCode;
            }
        }
    }
}
