namespace quickstartcore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using quickstartcore.Models;

    public class DocumentDBRepository<T> : IDocumentDBRepository<T> where T : class
    {
        private DocumentClient client;

        private readonly string DatabaseId;
        private readonly string CollectionId;

        private readonly string StoredProcedureBody = 
        @"function userProcedure() {
        var collection = getContext().getCollection();
        var response = getContext().getResponse();
        response.setBody(""Hello World"");
        }";

        private readonly string TriggerBody =
        @"function validate() {
        var context = getContext();
        var request = context.getRequest();                                                             
        var documentToCreate = request.getBody();
        
        // validate properties
        if (!('timestamp' in documentToCreate)) {
            var ts = new Date();
            documentToCreate['timestamp'] = ts.getTime();
        }
        
        // update the document that will be created
        request.setBody(documentToCreate);
        }";

        public DocumentDBRepository(AppSettings configuration)
        {
            client = new DocumentClient(new Uri(configuration.Endpoint), configuration.Key);
            DatabaseId = configuration.DatabaseId;
            CollectionId = configuration.CollectionId;

            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }
        
        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<Document> CreateItemAsync(T item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
        }

        public async Task<Document> UpdateItemAsync(string id, T item)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item);
        }

        public async Task DeleteItemAsync(string id)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<string> GetStoredProcedureResult()
        {
            var TestProc = new StoredProcedure
            {
                Id = "UserCreatedStoredProcedure",
                Body = StoredProcedureBody
            };

            try
            {
                var createdStoredProcedure = await client.CreateStoredProcedureAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), TestProc);
            }
            catch (DocumentClientException e)
            {
                Console.WriteLine(e.Message);
            }
            
            var storedProcedureResponse = await client.ExecuteStoredProcedureAsync<string>(UriFactory.CreateStoredProcedureUri(DatabaseId, CollectionId, TestProc.Id));

            return storedProcedureResponse;
        }

        public async Task<string> GetTriggerResult()
        {
            var trigger = new Trigger
            {
                Id = "UserCreatedTrigger",
                Body = TriggerBody,
                TriggerType = TriggerType.Pre,
                TriggerOperation = TriggerOperation.Create
            };

            // Todo make the trigger so something useful

            try
            {
                var createTrigger = await client.CreateTriggerAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), trigger);
            }
            catch (DocumentClientException e)
            {
                Console.WriteLine(e.Message);
            }

            return string.Empty;
        }
    }
}