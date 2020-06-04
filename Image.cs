using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Project
{
    /// <summary>
    /// An image
    /// </summary>
    public class Image
    {
        /// <summary>
        /// The image data
        /// </summary>
        private Image<Rgba32> _data;

        /// <summary>
        /// The image width
        /// </summary>
        public int Width { get { return _data.Width; } }

        /// <summary>
        /// The image height
        /// </summary>
        public int Height { get { return _data.Height; } }
        
        /// <summary>
        /// Gets the pixel value at the specified coordinates
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel</param>
        /// <param name="y">The y-coordinate of the pixel</param>
        /// <returns></returns>
        public Rgba32 GetPixel(int x, int y) => _data[x, y];

        /// <summary>
        /// Gets the 8-bit greyscale intensity of a pixel
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <returns>8-bit grayscale intensity</returns>
        public int GetPixelIntensity(int x, int y) => (int)((
            _data[x, y].R +
            _data[x, y].G +
            _data[x, y].B
        ) / 3);

        /// <summary>
        /// Sets the pixel value at the specified coordinates
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel</param>
        /// <param name="y">The y-coordinate of the pixel</param>
        /// <param name="newValue">The new value to assign to the pixel</param>
        public void SetPixel(int x, int y, Rgba32 newValue) => _data[x, y] = newValue;

        /// <summary>
        /// Create a new image using the image at the given filepath
        /// </summary>
        /// <param name="filePath">The filepath of an image file</param>
        public Image(string filePath)
        {
            _data = SixLabors.ImageSharp.Image.Load<Rgba32>(filePath);
        }

        /// <summary>
        /// Create a new image by copying an existing one
        /// </summary>
        /// <param name="image">An image object</param>
        public Image(Image image)
        {
            _data = image._data.Clone();
        }

        /// <summary>
        /// Create a new image using the given ImageSharp image object
        /// </summary>
        /// <param name="data">An ImageSharp Image object</param>
        public Image(Image<Rgba32> data)
        {
            _data = data.Clone();
        }

        /// <summary>
        /// Create a new empty (black) image from the given dimensions
        /// </summary>
        /// <param name="width">The desired width of the new image</param>
        /// <param name="height">The desired height of the new image</param>
        public Image(int width, int height)
        {
            _data = new Image<Rgba32>(Configuration.Default, width, height, Rgba32.Black);
        }

        /// <summary>
        /// Write the image to file with the PNG extension
        /// </summary>
        /// <param name="filePath">The desired filepath of the saved image excluding the extension</param>
        /// <returns>The full filepath written to, including the extension</returns>
        public string Write(string filePath)
        {
            _data.Save($"{filePath}.png");
            return $"{filePath}.png";
        }

        public override string ToString()
        {
            return string.Format("{0}x{1}", Width, Height);
        }

        /// <summary>
        /// Create a new image by resizing this one using NearestNeighbor interpolation
        /// </summary>
        /// <param name="image">The original image</param>
        /// <param name="newWidth">The new width for the image</param>
        /// <param name="newHeight">The new height for the image</param>
        /// <returns>A new resized image</returns>
        public static Image Resize(Image image, int newWidth, int newHeight)
        {
            ResizeOptions options = new ResizeOptions();
            options.Sampler = KnownResamplers.NearestNeighbor;
            options.Size = new SixLabors.Primitives.Size(newWidth, newHeight);
            Image newImage = new Image(image);
            newImage._data.Mutate(x => x.Resize(options));
            return newImage;
        }

        /// <summary>
        /// Create a new image by converting this one to greyscale
        /// </summary>
        /// <param name="image">The original image</param>
        /// <returns>A new greyscale image</returns>
        public static Image ToGrayscale(Image image)
        {
            return new Image(image._data.CloneAs<Gray8>().CloneAs<Rgba32>());
        }
    }
}
