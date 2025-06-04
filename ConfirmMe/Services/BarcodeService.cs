using ZXing;
using ZXing.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

namespace ConfirmMe.Services
{
    public class BarcodeService : IBarcodeService
    {
        public byte[] GenerateBarcode(string content, BarcodeFormat format = BarcodeFormat.CODE_128, int width = 300, int height = 100)
        {
            // Pakai BarcodeWriter langsung dari ZXing.ImageSharp
            var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
            {
                Format = format,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 10
                }
            };

            using var image = writer.Write(content); // Ini sudah hasilkan Image<Rgba32>
            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder()); // Simpan dalam format PNG
            return ms.ToArray(); // Kembalikan dalam bentuk byte[]
        }
    }
}
