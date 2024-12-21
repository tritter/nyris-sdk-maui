using Android.Content;
using Android.Graphics;

namespace Nyris.UI.Android.Custom
{
    public static class Utils
    {
        public static byte[] Optimize(this Bitmap bitmap)
        {
            var proportionalSize = CalculateProportionalSize(bitmap.Width, bitmap.Height);
            return Bitmap.CreateScaledBitmap(
                bitmap, proportionalSize.Item1, proportionalSize.Item2, true
            ).ToByteArray();
        }

        public static Bitmap ToBitmap(this byte[] image)
        {
            return BitmapFactory.DecodeByteArray(image, 0, image.Length);
        }

        public static Tuple<int, int> CalculateProportionalSize(int width, int height)
        {
            const int maxImageSize = 1024;
            if (width < maxImageSize && height < maxImageSize)
            {
                return new Tuple<int, int>(width, height);
            }

            var aspectRationWidth = maxImageSize / (float) width;
            var aspectRationHeight = maxImageSize / (float) height;
            var aspectRation = aspectRationWidth > aspectRationHeight ? aspectRationHeight : aspectRationWidth;
            return new Tuple<int, int>((int)(width * aspectRation), (int)(height * aspectRation));
        }

        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                // Compress the bitmap into the stream using the provided format and quality
                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);

                // Return the byte array from the stream
                return stream.ToArray();
            }
        }

        public static String DefaultPathForLastTakenImage(Context context)
        {
            var directory = new Java.IO.File(context.FilesDir, "searcher");
            var file = new Java.IO.File(directory, $"last.jpg");
            return file.AbsolutePath;
        }
        
        public static void SaveImageToInternalStorage(this byte[] image, Context context)
        {
            try
            {
                // Create or open a directory inside the app's internal storage
                var directory = new Java.IO.File(context.FilesDir, "searcher");
                if (!directory.Exists())
                {
                    directory.Mkdirs(); // Create the directory if it doesn't exist
                }
                
                // Write the byte array to the file
                var file = new Java.IO.File(directory, $"last.jpg");
                File.WriteAllBytes(file.AbsolutePath, image);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
            }
        }
    }
}
