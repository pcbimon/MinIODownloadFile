using System.IO;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace MinIODownloadFile
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var endpoint = "ENDPOINT";
            var port = 80;
            var accessKey = "YOUR_ACCESS_KEY";
            var secretKey = "YOUR_SECRET_KEY";
            try
            {
                var minio = new MinioClient()
                    .WithEndpoint(endpoint, port)
                    .WithCredentials(accessKey, secretKey)
                    // .WithSSL(false)
                    .Build();
                Run(minio).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
        // File uploader task.
        private async static Task Run(IMinioClient minio)
        {
            var bucketName = "bucketName";
            var location = "us-east-1";
            var objectName = "objectFileName.txt";
            var contentType = "text/plain";
            var outputFileName = "output.txt";
            try
            {
                MemoryStream fileMemoryStream = new MemoryStream();
                //confirm object exist
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);
                await minio.StatObjectAsync(statObjectArgs);

                // get input stream
                GetObjectArgs getObjectArgs = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream((stream) =>
                    {
                        stream.CopyTo(fileMemoryStream);
                    });
                await minio.GetObjectAsync(getObjectArgs);
                //seek position to 0
                fileMemoryStream.Position = 0;
                // save file to physical path
                using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    fileMemoryStream.CopyTo(fileStream);
                }
                Console.WriteLine("Successfully downloaded " + objectName);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Downloaded Error: {0}", e.Message);
            }
        }
    }
}
