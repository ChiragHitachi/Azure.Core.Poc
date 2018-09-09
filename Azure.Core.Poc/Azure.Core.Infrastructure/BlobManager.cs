using Azure.Core.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
 
namespace Verito.Infrastructure.Azure
{
    /// <summary>
    /// Allows ability to Upload and Download Blobs 
    /// </summary>
    public class AzureBlobStorage 
    {
        #region Private Field
        protected CloudStorageAccount cloudStorageAccount;
        protected CloudBlobClient cloudBlobClient;
        protected CloudBlobContainer cloudBlobContainer;
        #endregion
 
        public AzureBlobStorageSetting Settings { get; set; }
        protected bool isInitialized = false;
 
        public static readonly string AZUREBLOBCONNECTIONSTRING = "AzureBlobConnectionString";
        public static readonly string AZUREBLOBCONTAINERNAME = "AzureBlobContainerName";
 
        #region Constructor
 
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="settings">Settings required to call Azure Blob Service</param>
        public AzureBlobStorage(AzureBlobStorageSetting settings)
        {
            //Read the Azure Blob ConnectionString value from config file
            if (string.IsNullOrEmpty(settings.BlobConnectionString))
                throw new ArgumentNullException("Azure Blob ConnectionString is empty or null");
 
            //Read the Azure Blob Container Name from config file
            if (string.IsNullOrWhiteSpace(settings.BlobContainerName))
                throw new ArgumentNullException("Azure Blob Container Name is empty or null.");
 
            Settings = settings;
 
            //Initialize Cloud Client Object using AzureBlobConnectionString & AzureBlobContainer
            // DO NOT WRITE CODE That Delays Object Construction or has some chance of throwing an Exception
            //InitializeCloudClient(settings.AzureBlobConnectionString, settings.AzureBlobContainerName);
        }
        #endregion
 
        #region Public Method
 
        /// <summary>
        /// Save the Stream into cloud blob
        /// </summary>
        /// <param name="contentStream">Stream Object</param>
        /// <param name="blobName">Unique Blob Name</param>
        /// <returns>Unique Blob Name</returns>
        public async Task<string> CreateAsync(Stream contentStream, string blobName = "")
        {
            // Input Content Stream cannot be null as then there is nothing to actually upload
            if (contentStream == null)
                throw new ArgumentNullException("Input Content Stream is null");
 
            // If the client is not initialized, we need to initialize it.
            if (!isInitialized)
                Init();
 
            // Create a new GUID key if a Blob Name is not provided.
            if (string.IsNullOrEmpty(blobName))
                blobName = Guid.NewGuid().ToString();
 
            try
            {
                contentStream.Seek(0, SeekOrigin.Begin);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
 
                //Upload the Content Stream into cloud blob
                await cloudBlockBlob.UploadFromStreamAsync(contentStream);
            }
            catch (Exception ex)
            {
                throw;
            }
 
            return blobName;
        }
 
        /// <summary>
        /// Save the object by serializing and persiting it.
        /// </summary>
        /// <typeparam name="T">Type of Object to Store</typeparam>
        /// <param name="objectToStore">Type of Object to Store</param>
        /// <param name="blobName">Unique Blob Name</param>
        /// <returns>Unique Blob Name</returns>
        public async Task<string> SaveAsync<T>(T objectToStore, string blobName = "")
        {
            // T Type object cannot be null as then there is nothing to actually save            
            if (objectToStore == null)
                throw new ArgumentNullException("Received T Type object is null");
 
            // If the client is not initialized, we need to initialize it.
            if (!isInitialized)
                Init();
 
            // Create a new GUID key if a Blob Name is not provided.
            if (string.IsNullOrEmpty(blobName))
                blobName = Guid.NewGuid().ToString();
 
            try
            {
                // Convert the T Type object into Stream
                Stream stream = new MemoryStream();
                new BinaryFormatter().Serialize(stream, objectToStore);
 
                stream.Seek(0, SeekOrigin.Begin);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
 
                //Upload the Stream into cloud blob using Blob Name
                await cloudBlockBlob.UploadFromStreamAsync(stream);
            }
            catch (Exception ex)
            {
            }
 
            return blobName;
        }
 
        /// <summary>
        /// Retrieves the Blob and Deserializes the Object stored in Blob.
        /// </summary>
        /// <typeparam name="T">Type of Object to Retrieve</typeparam>
        /// <param name="blobName">Blob Name</param>
        /// <returns> T Type Object </returns>       
        public async Task<T> OpenAsync<T>(string blobName)
        {
            // Blob Name cannot be null as then there is nothing to actually get Requested T Type Object
            if (string.IsNullOrEmpty(blobName))
                throw new ArgumentNullException("Blob Name is null or empty");
 
            // If the client is not initialized, we need to initialize it.
            if (!isInitialized)
                Init();
 
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
 
                //if blob is not found in cloud then there is nothing to get Requested T Type Object
 
                //download the blob as stream using Blob Name
                await cloudBlockBlob.DownloadToStreamAsync(memoryStream);
 
                memoryStream.Position = 0;
                //convert the stream into the required T Object
                return (T)new BinaryFormatter().Deserialize(memoryStream);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
 
        /// <summary>
        /// Get the Blob Url by Blob Name
        /// </summary>
        /// <param name="blobName">Unique Blob Name</param>
        /// <returns>Blob Url</returns>
        public string GetAddressByName(string blobName)
        {
            // Blob Name cannot be null as then there is nothing to actually get blob url
            if (string.IsNullOrEmpty(blobName))
                throw new ArgumentNullException("Blob Name is null or empty");
 
            // If the client is not initialized, we need to initialize it.
            if (!isInitialized)
                Init();
 
            string blobUrl = string.Empty;
 
            try
            {
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
 
                //if blob is not found in cloud then there is nothing to get Absolute Uri of Blob
 
                //Get the Absolute Uri of the Blob using Blob Name
                blobUrl = cloudBlockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
            }
            return blobUrl;
        }
 
        /// <summary>
        /// Delete the blob from cloud
        /// </summary>
        /// <param name="blobName">Blob Name which you want to delete</param>
        /// <returns>if blob is successfully deleted from cloud then it will return True else False</returns>
        public async Task<bool> Delete(string blobName)
        {
            // Blob Name cannot be null as then there is nothing to delete anything
            if (string.IsNullOrEmpty(blobName))
                throw new ArgumentNullException("Blob Name is null or empty");
 
            bool isSuccessfullyDeleted = true;
            // If the client is not initialized, we need to initialize it.
            if (!isInitialized)
                Init();
 
            try
            {
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
 
                //if blob is not found in cloud then there is nothing to delete anything
                if (cloudBlockBlob != null)
 
                //delete the blob from cloud
                await cloudBlockBlob.DeleteAsync();
            }
            catch (Exception ex)
            {
                isSuccessfullyDeleted = false;
            }
            return isSuccessfullyDeleted;
        }
        #endregion
 
 
        #region Private Method   
        protected async void Init()
        {
            try
            {
                cloudStorageAccount = CloudStorageAccount.Parse(Settings.BlobConnectionString);
                cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                cloudBlobContainer = cloudBlobClient.GetContainerReference(Settings.BlobContainerName);
                await cloudBlobContainer.CreateIfNotExistsAsync();
                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                isInitialized = true;
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}