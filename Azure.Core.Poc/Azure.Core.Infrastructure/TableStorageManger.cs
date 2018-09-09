using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace Azure.Core.Infrastructure{
    public class TableStorageManager<T>
    {
        public string tableConnectionString;
        private CloudStorageAccount cloudStorageAccount;
        private CloudTable cloudTable;
        private CloudTableClient cloudTableClient;
        
        public TableStorageManager(string connectionString){
            this.tableConnectionString = connectionString;
        }

        private void Init(){
           this.cloudStorageAccount = CloudStorageAccount.Parse(this.tableConnectionString);
           this.cloudTableClient = this.cloudStorageAccount.CreateCloudTableClient();
           this.cloudTable = this.cloudTableClient.GetTableReference(typeof(T).Name);
           this.cloudTable.CreateIfNotExistsAsync().Wait();

        }

        public async Task<T> GetByKey(string rowKey, string partitionKey){
            try{
                 TableOperation tableOperation = TableOperation.Retrieve(partitionKey, rowKey);
                 TableResult tableResult = await this.cloudTable.ExecuteAsync(tableOperation);

                if (tableResult.Result != null)
                {
                    return (T)tableResult.Result;
                }
                else
                    throw new Exception();
            }
            catch (Exception ex){
                 throw; //TODO Handle Exception   
            }
        }

         public async Task<List<T>> GetMany(string partitionKey){
            try{
                TableContinuationToken continuationToken = null;
                TableQuery tableQuery = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                List<DynamicTableEntity> allRows = new List<DynamicTableEntity>();
                do{
                    TableQuerySegment tableQuerySegement = await this.cloudTable.ExecuteQuerySegmentedAsync(tableQuery, continuationToken).ConfigureAwait(false);
                    continuationToken = tableQuerySegement.ContinuationToken;
                    if(tableQuerySegement.Results != null && tableQuerySegement.Results.Count > 0)
                        allRows.AddRange(tableQuerySegement.Results);

                } while (continuationToken != null);
                return (List<T>)allRows.Cast<T>();
            }
            catch (Exception ex){
                 throw; //TODO Handle Exception   
            }
        }

    }
}


