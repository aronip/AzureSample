using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using Microsoft.WindowsAzure;
using System.Diagnostics;
using System.Drawing;
using GuestBook_Data;


namespace TestGuestBook
{
    /// <summary>
    /// Summary description for Storage
    /// </summary>
    [TestClass]
    public class Storage
    {       
        [TestMethod]
        //If pass, the fabric is already up and running
        public void FabricIsOn()
        {
            Assert.IsTrue(HelperObject.FabricLoaded());
        }

        [TestMethod]
        //If pass, you started and verified fabric is up and running
        public void StartDevFabric()
        {
            HelperObject.RunCsrun("/devfabric:start");
            HelperObject.RunCsrun("/devstore:start");

            Assert.IsTrue(HelperObject.FabricLoaded());
        }

        [TestMethod]
        //If pass, you could stop the fabric
        public void StopDevFabric()
        {
            HelperObject.RunCsrun("/devfabric:shutdown");
            HelperObject.RunCsrun("/devstore:shutdown");

            Assert.IsFalse(HelperObject.FabricLoaded());
        }

        [TestMethod]
        //If pass, you could create or get access to a blob and container and set permissions for it
        public void ValidateBlobStorageInit()
        {
            //stores our pictures
            CloudBlobClient blobStorage;
            SetupStorageAccountDevFabric();

            //create blob container for images
            blobStorage = StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobStorage.GetContainerReference("guestbookpics");
            Assert.AreEqual<string>(@"http://127.0.0.1:10000/devstoreaccount1/guestbookpics", container.Uri.ToString());
            Assert.AreEqual<string>(@"guestbookpics", container.Name.ToString());

            container.CreateIfNotExist();

            //configure container for public access

            var permissions = container.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            Assert.AreNotEqual(permissions.PublicAccess, null);

            container.SetPermissions(permissions);
        }

        [TestMethod]
        //If pass, you could create an Azure queue object
        public void ValidateQueue()
        {
            CloudQueueClient queueStorage = null;
            SetupStorageAccountDevFabric();
            
            //create queue to communicate with worker role
            queueStorage = StorageAccount.CreateCloudQueueClient();
            Assert.AreNotEqual(queueStorage, null);
            
            CloudQueue queue = queueStorage.GetQueueReference("guestthumbs");

            queue.CreateIfNotExist();

            Assert.AreEqual<string>(@"http://127.0.0.1:10001/devstoreaccount1/guestthumbs", queue.Uri.ToString());
           
        }

        [TestMethod]
        //If pass, I was able to upload an arbirary file.
        public void ValidateUploadBlob()
        {
            CloudBlobClient blobStorage = null;
            CloudBlobContainer container = null;
            CloudBlockBlob blob = null;

            UploadBlob(blobStorage, container, blob);
            

        }

        public void UploadBlob(CloudBlobClient blobStorage, CloudBlobContainer container, CloudBlockBlob blob)
        {
            SetupStorageAccountDevFabric();

            //create blob container for images
            blobStorage = StorageAccount.CreateCloudBlobClient();
            container = blobStorage.GetContainerReference("guestbookpics");
            Assert.AreEqual<string>(@"http://127.0.0.1:10000/devstoreaccount1/guestbookpics", container.Uri.ToString());
            Assert.AreEqual<string>(@"guestbookpics", container.Name.ToString());

            container.CreateIfNotExist();

            //configure container for public access

            var permissions = container.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            Assert.AreNotEqual(permissions.PublicAccess, null);

            container.SetPermissions(permissions);

            string uniqueBlobName = string.Format("image.jpg");
            blob = container.GetBlockBlobReference(uniqueBlobName);

            FileInfo fi = new FileInfo(@"C:\Users\niaro\Desktop\hires.jpg");

            string ext = Path.GetExtension(fi.Name).ToLower();
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

            if(rk!=null && rk.GetValue("Content Type") !=null)
                blob.Properties.ContentType = rk.GetValue("Content Type").ToString();
            
            blob.DeleteIfExists();
            
            using (FileStream inFile = fi.OpenRead())
                blob.UploadFromStream(inFile);


            Assert.AreEqual<string>(@"http://127.0.0.1:10000/devstoreaccount1/guestbookpics/image.jpg", blob.Uri.ToString());
          
        }

        [TestMethod]
        //If pass, I was able to add an entity into the table storage. 
        public void ValidateTables()
        {
            GuestBookDataContext context;
            SetupStorageAccountDevFabric();
            string timeStamp = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString();
            timeStamp = timeStamp.Replace(":", "_");
            timeStamp = timeStamp.Replace("/", "_");
            timeStamp = timeStamp.Replace(" ", "_");

            GuestBookEntry entry = new GuestBookEntry()
            {
                GuestName = "Test Name",
                Message = "Test Message",
                PhotoUrl = "image.jpg",
                ThumbnailUrl = "the URI goes here" + timeStamp
            };

            CloudTableClient.CreateTablesFromModel(typeof(GuestBookDataContext), StorageAccount.TableEndpoint.AbsoluteUri, StorageAccount.Credentials);

            context = new GuestBookDataContext(StorageAccount.TableEndpoint.AbsoluteUri, StorageAccount.Credentials);
            context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
            context.AddObject("GuestBookEntry", entry);

            Assert.AreEqual<string>(@"http://127.0.0.1:10002/devstoreaccount1",
                                    context.BaseUri.ToString());

            context.SaveChanges();

        }

        [TestMethod]
        //Unit Test to delete table entries one by one.
        public void DeleteTable()
        {
            GuestBookDataContext context;
            SetupStorageAccountDevFabric();  // sets up StorageAccount

            GuestBookEntry entry = new GuestBookEntry();

            CloudTableClient.CreateTablesFromModel(
                        typeof(GuestBookDataContext),
                        StorageAccount.TableEndpoint.AbsoluteUri,
                        StorageAccount.Credentials);

            context = new GuestBookDataContext(StorageAccount.TableEndpoint.AbsoluteUri,
                                                  StorageAccount.Credentials);


            var items = (from i in context.GuestBookEntry
                         select i);

            foreach (GuestBookEntry w in items)
            {
                context.DeleteObject(w);
                context.SaveChangesWithRetries();

            }


            items = (from i in context.GuestBookEntry
                     select i);
            var item = items.SingleOrDefault<GuestBookEntry>();
            Assert.IsTrue(item == null);

            int count = 0;
            foreach (GuestBookEntry w in items)
            {
                count++;
            }
            Assert.IsTrue(count == 0);
        }

        [TestMethod]
        //Unit test to insert a message to the queue.
        public void ValidateQueue2()
        {
            DeleteTable();
            ValidateUploadBlob();
            ValidateTables();

            SetupStorageAccountDevFabric();
            CloudQueue queue = null;
            GuestBookDataContext context;

            GuestBookEntry entry = new GuestBookEntry();

            CloudTableClient.CreateTablesFromModel(typeof(GuestBookDataContext),StorageAccount.TableEndpoint.AbsoluteUri,StorageAccount.Credentials);

            context = new GuestBookDataContext(StorageAccount.TableEndpoint.AbsoluteUri, StorageAccount.Credentials);

            var items = (from i in context.GuestBookEntry select i);
            var item = items.FirstOrDefault<GuestBookEntry>();
            Assert.IsFalse(item == null);

            CloudQueueClient queueStorage;
            queueStorage = StorageAccount.CreateCloudQueueClient();
            Assert.AreNotEqual(queueStorage, null);
            
            queue = queueStorage.GetQueueReference("guestthumbs");
            queue.Clear();
            string uniqueBlobName = "image.jpg";

            var message = new CloudQueueMessage(String.Format("{0},{1},{2}", uniqueBlobName, item.PartitionKey, item.RowKey));
            queue.AddMessage(message);

            Assert.AreEqual<string>(queue.Uri.AbsoluteUri.ToString(), "http://127.0.0.1:10001/devstoreaccount1/guestthumbs");

            Assert.AreEqual<string>(queue.Uri.LocalPath.ToString(), "/devstoreaccount1/guestthumbs");
        }

        [TestMethod]
        public void ProcessQueue()
        {
            GuestBookDataContext context = null;
            CloudQueue queue = null;

            SetupStorageAccountDevFabric();
            CloudBlobClient blobStorage = StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobStorage.GetContainerReference("guestbookpics");
            Assert.AreEqual<string>(@"http://127.0.0.1:10000/devstoreaccount1/guestbookpics", container.Uri.ToString());
            Assert.AreEqual<string>(@"guestbookpics", container.Name.ToString());
            
            
            CloudQueueClient queueStorage = StorageAccount.CreateCloudQueueClient();
            
            Trace.TraceInformation("Creating container and queue...");

            bool storageInitialized = false;
            // Loop until we get our storage initialized
            while (!storageInitialized)
            {
                try
                {
                    // Create the blob container (if does not exist) and allow public access
                    container.CreateIfNotExist();
                    var permissions = container.GetPermissions();
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    container.SetPermissions(permissions);

                    // Create a queue to hold messages for our worker role to consume
                    queueStorage = StorageAccount.CreateCloudQueueClient();
                    Assert.AreNotEqual(queueStorage, null);

                    ///////////////////////////////////////////////////////////////////////////
                    // Code Part 3
                    // Get a reference to our queue to read/write from/to
                    queue = queueStorage.GetQueueReference("guestthumbs");
                    Assert.AreEqual<string>(queue.Uri.ToString(), @"http://127.0.0.1:10001/devstoreaccount1/guestthumbs");

                    // Create the message queue if does not exist
                    queue.CreateIfNotExist();
                    // Quit out of the loop, mission accomplished
                    storageInitialized = true;
                }
                catch (StorageClientException e)
                {
                    if (e.ErrorCode == StorageErrorCode.TransportError)
                    {
                        Trace.TraceError("Storage services initialization failure. "
                            + "Check your storage account configuration settings. If running locally, "
                            + "ensure that the Development Storage service is running. Message: '{0}'", e.Message);
                        System.Threading.Thread.Sleep(5000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ///////////////////////////////////////////////////////////////////////////
            // Code Part 4
            // Get a reference to our queue to read/write from/to
            // Get ready to select data from Azure tables

            // Create a Simple object that reprents our data for the 
            //    Azure tables ( GuestName, Message, PhotoUrl, ThumbnailUrl)
            GuestBookEntry item = new GuestBookEntry();

            CloudTableClient.CreateTablesFromModel(
                        typeof(GuestBookDataContext),
                        StorageAccount.TableEndpoint.AbsoluteUri,
                        StorageAccount.Credentials);

            ///////////////////////////////////////////////////////////////////////////
            // Code Part 5
            // Connect to table endpoint
            context = new GuestBookDataContext(StorageAccount.TableEndpoint.AbsoluteUri,
                                                    StorageAccount.Credentials);

            Assert.IsNotNull(context);

            // Select all the records from Azure Table
            var items = (from i in context.GuestBookEntry
                         select i);
            Assert.IsNotNull(items);

            // Grab the first one.
            item = items.FirstOrDefault<GuestBookEntry>();



            ///////////////////////////////////////////////////////////////////////////
            // Code Part 6
            // If no records, then create a record to test with
            if (item == null)
            {
                // We'll supply a temporary time stamp for our "ThumbnailUrl"
                string timeStamp = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString();
                timeStamp = timeStamp.Replace(":", "_");
                timeStamp = timeStamp.Replace("/", "_");
                timeStamp = timeStamp.Replace(" ", "_");


                ///////////////////////////////////////////////////////////////////////////
                // Code Part 7
                // Create an entity so we can do an insert
                GuestBookEntry entry = new GuestBookEntry()
                {
                    GuestName = "Bruno Guest Name",
                    Message = "Bruno Message",
                    PhotoUrl = "image.jpg",
                    ThumbnailUrl = "the URI goes here" + timeStamp
                };
                ///////////////////////////////////////////////////////////////////////////
                // Code Part 8
                // Set the try policy and do the actual insert
                context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
                context.AddObject("GuestBookEntry", entry);
                Assert.AreEqual<string>(@"http://127.0.0.1:10002/devstoreaccount1",
                                        context.BaseUri.ToString());
                context.SaveChanges();

                // Now that we have data, let's select it back out
                items = (from i in context.GuestBookEntry
                         select i);

                // "item" 
                item = items.FirstOrDefault<GuestBookEntry>();
                Assert.IsNotNull(item);

            }

            Assert.IsNotNull(item); // if failed, then table is empty, must have been an error

            ///////////////////////////////////////////////////////////////////////////
            // Code Part 9
            // Grab the unique "PhotoUrl" of our inserted image
            string uniqueBlobName = item.PhotoUrl.ToString();

            // Clear out the queue
            queue.Clear();

            ///////////////////////////////////////////////////////////////////////////
            // Code Part 10
            // Compose a message that we will add to the queue
            var message = new
                CloudQueueMessage(String.Format("{0},{1},{2}",
                                    uniqueBlobName,
                                    item.PartitionKey,
                                    item.RowKey));

            ///////////////////////////////////////////////////////////////////////////
            // Code Part 11
            // Add the message to the queue
            queue.AddMessage(message);

            Assert.AreEqual<string>(queue.Uri.AbsoluteUri.ToString(), "http://127.0.0.1:10001/devstoreaccount1/guestthumbs");
            Assert.AreEqual<string>(queue.Uri.LocalPath.ToString(), "/devstoreaccount1/guestthumbs");




            ///////////////////////////////////////////////////////////////////////////
            // Code Part 12
            bool keep_going = true;
            // Loop, making thumbnails while the queue has messages to read
            while (keep_going)
            {
                try
                {
                    // Retrieve a new message from the queue
                    CloudQueueMessage msg = queue.GetMessage();

                    ///////////////////////////////////////////////////////////////////////////
                    // Code Part 13
                    // If message is there, process it. If no message, sleep and try again soon.
                    if (msg != null)
                    {
                        // Parse message retrieved from queue. It is just a comma-separated string
                        var messageParts = msg.AsString.Split(new char[] { ',' });
                        var uri = messageParts[0];
                        var partitionKey = messageParts[1];
                        var rowkey = messageParts[2];
                        Trace.TraceInformation("Processing image in blob '{0}'.", uri);

                        // Download original image from blob storage. We cant make a thumbnail if we 
                        // don't have the full fidelity photo
                        CloudBlockBlob imageBlob = container.GetBlockBlobReference(uri); // Data type to stream an image to 
                        // client
                        Assert.IsNotNull(imageBlob);

                        MemoryStream image = new MemoryStream();                         // Data structure to hold image in memory
                        imageBlob.DownloadToStream(image);                               // Download to client
                        image.Seek(0, SeekOrigin.Begin);

                        ///////////////////////////////////////////////////////////////////////////
                        // Code Part 14
                        // Create a thumbnail image and upload into a blob. Just add "_thumb" to the filename
                        // Grab the uri from the cloud. 
                        string thumbnailUri = String.Concat(Path.GetFileNameWithoutExtension(uri), "_thumb.jpg");
                        Assert.AreEqual<string>(thumbnailUri, "image_thumb.jpg");
                        CloudBlockBlob thumbnailBlob = container.GetBlockBlobReference(thumbnailUri);
                        string ext = Path.GetExtension(uri).ToLower();
                        Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

                        if (rk != null && rk.GetValue("Content Type") != null)
                            thumbnailBlob.Properties.ContentType = rk.GetValue("Content Type").ToString();
                        thumbnailBlob.UploadFromStream(CreateThumbnail(image));

                        // Update the entry in table storage to point to the thumbnail. Notice that we ultimately
                        // save an object with Azure tables ( GuestName, Message, PhotoUrl, ThumbnailUrl)
                        // 
                        // Prepare a data for use, async if desired. Other functions include Delete, Check if exist, List tables
                        CloudTableClient.CreateTablesFromModel(
                            typeof(GuestBookDataContext),
                            StorageAccount.TableEndpoint.AbsoluteUri,
                            StorageAccount.Credentials);
                        context = new GuestBookDataContext(StorageAccount.TableEndpoint.AbsoluteUri, StorageAccount.Credentials);
                        context.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

                        ///////////////////////////////////////////////////////////////////////////
                        // Code Part 15
                        // Select from the table using the partition and row keys
                        var results = from g in context.GuestBookEntry
                                      where g.PartitionKey == item.PartitionKey && g.RowKey == item.RowKey
                                      select g;

                        // Will return one row only. But "Default" helps because will give you a null object
                        var entry = results.FirstOrDefault<GuestBookEntry>();

                        // The whole point is to update the "ThumbnailUrl" column with the path of the new 
                        // thumbnail file
                        entry.ThumbnailUrl = thumbnailBlob.Uri.AbsoluteUri;
                        context.UpdateObject(entry);  // You need both an "Update" and a "Save"
                        context.SaveChanges();

                        // Remove message from queue. Since thumbnail is done, remove queue message.
                        queue.DeleteMessage(msg);
                        keep_going = false;

                        Trace.TraceInformation("Generated thumbnail in blob '{0}'.", thumbnailBlob.Uri);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (StorageClientException e)
                {
                    Trace.TraceError("Exception when processing queue item. Message: '{0}'", e.Message);
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }
        private Stream CreateThumbnail(Stream input)
        {
            var orig = new Bitmap(input);
            int width;
            int height;

            if (orig.Width > orig.Height)
            {
                width = 128;
                height = 128 * orig.Height / orig.Width;
            }
            else
            {
                height = 128;
                width = 128 * orig.Width / orig.Height;
            }

            var thumb = new Bitmap(width, height);

            using (Graphics graphic = Graphics.FromImage(thumb))
            {
                graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphic.DrawImage(orig, 0, 0, width, height);
                var ms = new MemoryStream();
                thumb.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }
        protected static CloudStorageAccount StorageAccount
        { get; set; }

        public static void SetupStorageAccountDevFabric()
        {
            if (!HelperObject.FabricLoaded())
            {
                HelperObject.RunCsrun("/devfabric:start");
                HelperObject.RunCsrun("/devstore:start");
            }
            StorageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
        }


    }
}
