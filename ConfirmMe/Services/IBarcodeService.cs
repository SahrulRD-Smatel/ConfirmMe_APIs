using ZXing;
using ZXing.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

namespace ConfirmMe.Services
{
    public interface IBarcodeService
    {
        byte[] GenerateBarcode(string data, ZXing.BarcodeFormat format, int width, int height);
    }

}
