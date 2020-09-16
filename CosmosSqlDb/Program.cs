using CosmosSqlDb.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosSqlDb
{
    class Program
    {

        private readonly string EndpointUrl = Environment.GetEnvironmentVariable("CosmosSqlUrl");

        private readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosSqlPrimaryKey");

        private CosmosClient cosmosClient;

        private Database database;

        private Container container;

        private string databaseId = "CompanyDatabase";
        private string containerId = "CompanyContainer";

        static async Task Main(string[] args)
        {
            Program p = new Program();
            await p.CreateDemoAsync();
            await p.QueryDemoAsync();
            await p.DeleteDemoAsync();
        }

        public async Task CreateDemoAsync()
        {
            cosmosClient = new CosmosClient(EndpointUrl, PrimaryKey);

            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine($"Created Database: {database.Id}");

            container = await database.CreateContainerIfNotExistsAsync(containerId, "/Name");
            Console.WriteLine($"Created Container: {container.Id}");

            Company itixoCompany = CreateCompany("Itixo");

            try
            {
                ItemResponse<Company> itixoCompanyResponse = await container.CreateItemAsync<Company>(itixoCompany, new PartitionKey(itixoCompany.Name));
                Console.WriteLine($"Created item in database with id: {itixoCompanyResponse.Resource.Id} Operation consumed {itixoCompanyResponse.RequestCharge} RUs.");

            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Console.WriteLine($"Item in DB with id {itixoCompany.Id} already exists");
            }
        }

        public async Task QueryDemoAsync()
        {
            var query = "SELECT * FROM c WHERE c.Name = 'Itixo'";

            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<Company> queryResultIterator = container.GetItemQueryIterator<Company>(queryDefinition);

            while (queryResultIterator.HasMoreResults)
            {
                FeedResponse<Company> companies = await queryResultIterator.ReadNextAsync();

                foreach (Company company in companies)
                {
                    Console.WriteLine(company);
                }
            }
        }

        public async Task DeleteDemoAsync()
        {
            DatabaseResponse dbResponse = await database.DeleteAsync();
            cosmosClient.Dispose();
        }

        private Company CreateCompany(string companyName)
        {
            return new Company
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                BusinessField = BusinessField.IT,
                Employees = new List<Employee>
                {
                    new Employee
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Position = JobPosition.Ceo
                    },
                    new Employee
                    {
                        FirstName = "Jack",
                        LastName = "Black",
                        Position = JobPosition.SwDeveloper
                    }
                }
            };
        }
    }
}
