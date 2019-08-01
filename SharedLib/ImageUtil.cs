using System;
using System.IO;
using System.Threading.Tasks;

#if __IOS__
using System.Drawing;
using UIKit;
using CoreGraphics;
#endif

#if __ANDROID__
using Android.Graphics;
#endif

#if WINDOWS_UWP
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

// Code from https://github.com/xamarin/xamarin-forms-samples/blob/master/XamFormsImageResize/XamFormsImageResize/ImageResizer.cs

namespace Magnolia.Xamarin
{
    public static partial class ImageUtil
    {


        public static async Task<byte[]> ResizeImage(byte[] imageData, float width, float height)
        {
            if( imageData == null || imageData.Length == 0) {
                return imageData;
            }

#if __IOS__
            return ResizeImageIOS(imageData, width, height);
#endif
#if __ANDROID__
			return ResizeImageAndroid ( imageData, width, height );
#endif
#if WINDOWS_UWP
            var resizedBytes = await ResizeImageWindows(imageData, width, height);
            return resizedBytes;
#endif
            return null;
        }


#if __IOS__
        public static byte[] ResizeImageIOS(byte[] imageData, float width, float height)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            UIImageOrientation orientation = originalImage.Orientation;
            
            var aspectRatio = (float)originalImage.Size.Width / (float)originalImage.Size.Height;
            if (width == 0 && height > 0) {
                width = height / aspectRatio;
            } else if (height == 0 && width > 0) {
                height = width * aspectRatio;
            }

            //create a 24bit RGB image
            using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
                                                 (int)width, (int)height, 8,
                                                 4 * (int)width, CGColorSpace.CreateDeviceRGB(),
                                                 CGImageAlphaInfo.PremultipliedFirst))
            {
                RectangleF imageRect = new RectangleF(0, 0, width, height);

                // draw the image
                context.DrawImage(imageRect, originalImage.CGImage);

                UIKit.UIImage resizedImage = UIKit.UIImage.FromImage(context.ToImage(), 0, orientation);

                // save the image as a png
                return resizedImage.AsPNG().ToArray();
            }
        }

        public static UIKit.UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            UIKit.UIImage image;
            try
            {
                image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }
#endif

#if __ANDROID__
		
		public static byte[] ResizeImageAndroid (byte[] imageData, float width, float height)
		{
			// Load the bitmap
			Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            var aspectRatio = originalImage.Width / originalImage.Height;
            if (width == 0 && height > 0) {
                width = height / aspectRatio;
            } else if (height == 0 && width > 0) {
                height = width * aspectRatio;
            }
			Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);

			using (MemoryStream ms = new MemoryStream())
			{
				resizedImage.Compress (Bitmap.CompressFormat.Png, 100, ms);
				return ms.ToArray ();
			}
		}

#endif

#if WINDOWS_UWP

        public static async Task<byte[]> ResizeImageWindows(byte[] imageData, float width, float height)
        {
            byte[] resizedData;

            using (var streamIn = new MemoryStream(imageData))
            {
                using (var imageStream = streamIn.AsRandomAccessStream())
                {
                    var decoder = await BitmapDecoder.CreateAsync(imageStream);
                    var aspectRatio = (float)decoder.PixelWidth / (float)decoder.PixelHeight;
                    if (width == 0 && height > 0) {
                        width = height / aspectRatio;
                    } else if (height == 0 && width > 0) {
                        height = width * aspectRatio;
                    }
                    var resizedStream = new InMemoryRandomAccessStream();
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                    encoder.BitmapTransform.ScaledHeight = (uint)height;
                    encoder.BitmapTransform.ScaledWidth = (uint)width;
                    await encoder.FlushAsync();
                    resizedStream.Seek(0);
                    resizedData = new byte[resizedStream.Size];
                    await resizedStream.ReadAsync(resizedData.AsBuffer(), (uint)resizedStream.Size, InputStreamOptions.None); 
                }                
            }

            return resizedData;
        }
#endif



    }
}