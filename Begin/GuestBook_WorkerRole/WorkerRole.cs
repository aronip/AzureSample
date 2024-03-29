using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using GuestBook_Data;
using Microsoft.WindowsAzure.StorageClient;


namespace GuestBook_WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private CloudQueue queue;
        private CloudBlobContainer container;
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("Listening for queue messages");

            while (true)
            {
                try
                {

                    // retrieve a message from queue
                    CloudQueueMessage msg = queue.GetMessage();
                    if (msg != null)
                    {
                        //parse message retrieved from queue
                        var messageParts = msg.AsString.Split(new char[] { ',' });
                        var imageBlobUri = messageParts[0];
                        var partitionKey = messageParts[1];
                        var rowKey = messageParts[2];

                        Trace.TraceInformation("Processing image in blob '{0}'", imageBlobUri);
                        string thumbnailBlobUri = System.Text.RegularExpressions.Regex.Replace(imageBlobUri, "([^\\.]+)(\\.[^\\.]+)?$", "$1-thumb$2");
                        CloudBlob inputBlob = container.GetBlobReference(imageBlobUri);
                        CloudBlob outputBlob = container.GetBlobReference(thumbnailBlobUri);
                        using (BlobStream input = inputBlob.OpenRead())
                        using (BlobStream output = outputBlob.OpenWrite())
                        {
                            ProcessImage(input, output);
                            //commit the blob and set its properties
                            output.Commit();
                            outputBlob.Properties.ContentType = "image/jpeg";
                            outputBlob.SetProperties();

                            //update the entry in table storage to point to the thumbnail
                            GuestBookDataSource ds = new GuestBookDataSource();
                            ds.UpdateImageThumbnail(partitionKey, rowKey, thumbnailBlobUri);

                        }

                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (StorageClientException e) 
                {
                    Trace.TraceError("Exception when processing queue item. Message: '{0}'", e.Message);
                    Thread.Sleep(5000);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            //read storage account configuration settings
            Microsoft.WindowsAzure.CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) => 
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            });
            var storageAccount = Microsoft.WindowsAzure.CloudStorageAccount.FromConfigurationSetting("DataConnectionString");

            //initialize blob storage
            CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
            container = blobStorage.GetContainerReference("guestbookpics");

            //initialze queue storage
            CloudQueueClient queueStorage = storageAccount.CreateCloudQueueClient();
            queue = queueStorage.GetQueueReference("guestthumbs");
            Trace.TraceInformation("Creating Container and queue...");
            bool storageInitialized = false;
            while (!storageInitialized)
            {
                try
                {
                    //create the blob container and allow public access
                    container.CreateIfNotExist();
                    var permissions = container.GetPermissions();
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    container.SetPermissions(permissions);

                    //create the message queue
                    queue.CreateIfNotExist();
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
            return base.OnStart();
        }

        public void ProcessImage(Stream input, Stream output)
        {
            int width, height;
            var originalImage = new Bitmap(input);
            if (originalImage.Width > originalImage.Height)
            {
                width = 128;
                height = 128 * originalImage.Height / originalImage.Width;
            }
            else
            {
                height = 128;
                width = 128 * originalImage.Width / originalImage.Height;
            }
            var thumbnailImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(thumbnailImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(originalImage, 0, 0, width, height);
            }
            thumbnailImage.Save(output, ImageFormat.Jpeg);
        }

    }
}
