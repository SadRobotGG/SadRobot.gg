using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SadRobot.Functions.Media;
using SixLabors.ImageSharp;

namespace SadRobot.Functions
{
    public static class BlpConverter
    {
        [FunctionName("BlpConverter")]
        public static void Run([BlobTrigger("wow/{name}.blp")]Stream input, string name, [Blob("wow/{name}.png", FileAccess.Write)]Stream output, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {input.Length} Bytes");

            using (var blp = new BlpFile(input))
            using (var image = blp.GetImage(0))
            using (var file = new MemoryStream())
            {
                image.SaveAsPng(output);
            }
        }
    }
}
