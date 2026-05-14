using PdfiumViewer;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;

namespace rupbes.Classes
{
    public static class PdfToImageHelper
    {
        /// <summary>
        /// Конвертирует PDF файл в список изображений (постранично)
        /// Работает на .NET Framework 4.6.1
        /// </summary>
        /// <param name="pdfStream">Поток с PDF файлом</param>
        /// <param name="dpi">Разрешение (рекомендуется 150-300)</param>
        /// <returns>Список байтов изображений в формате JPEG</returns>
        public static List<byte[]> ConvertPdfToImages(Stream pdfStream, int dpi = 150)
        {
            var images = new List<byte[]>();

            using (var pdfDocument = PdfDocument.Load(pdfStream))
            {
                for (int page = 0; page < pdfDocument.PageCount; page++)
                {
                    // Рендерим страницу в изображение
                    using (var image = pdfDocument.Render(page, dpi, dpi, true))
                    {
                        using (var ms = new MemoryStream())
                        {
                            // Сохраняем как JPEG с качеством 90%
                            var jpegCodec = GetEncoderInfo("image/jpeg");
                            var encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                            image.Save(ms, jpegCodec, encoderParams);
                            images.Add(ms.ToArray());
                        }
                    }
                }
            }

            return images;
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            foreach (var codec in codecs)
            {
                if (codec.MimeType == mimeType)
                    return codec;
            }
            return null;
        }
    }
}
