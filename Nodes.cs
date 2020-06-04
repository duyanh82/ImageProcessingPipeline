using System;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// A set of nodes used to manipulate images
/// </summary>
namespace Project
{ 
    /// <summary>
    /// Parent class of nodes
    /// </summary>
    public abstract class Node
    {
        public abstract Image Process(Image input);
        public abstract string Name();
        public virtual string OtherInfo() {return "";}
    }

    /// <summary>
    /// This class is used to process input using a list of nodes and apply them to image
    /// </summary>
    class Pipeline
    {
        /// <summary>
        /// This method is used to apply nodes to image
        /// </summary>
        /// <param name="input"> Input image </param>
        /// <param name="pipeline"> List of nodes </param>
        /// <param name="logging"> Show the details of image through each node </param>
        /// <param name="saveDir"> A directory name to save intermediate images </param>
        /// <returns></returns>
        public static Image Run(Image      input, 
                                List<Node> pipeline, 
                                bool       logging = false, 
                                bool       saveInter = false,
                                string     saveDir = "")
        {
            if (logging)
                Console.WriteLine($"Running pipeline on the image...\n");

            // Processing nodes and show loggings if required
            for (int i = 0; i < pipeline.Count; i++)
            {
                if (logging)
                {
                    // Show the processing node name
                    Console.WriteLine($"{"Node:",20} {pipeline[i].Name()} {pipeline[i].OtherInfo()}"); 
                    // Show the input dimensions of intermediate image
                    Console.WriteLine($"{"Input dimensions:",20} {input.ToString()}");
                }
                // Apply node to the image
                input = pipeline[i].Process(input);
                // A string to hold intermediate image name
                string imageName = $"{saveDir}/output{i+1}";

                // Check the availability of saveDir, create a new folder if it does not exist
                if (!Directory.Exists(saveDir)) 
                    Directory.CreateDirectory(saveDir);

                // Print logging if requested
                if (logging)
                    // Show the output dimensions of intermediate image
                    Console.WriteLine($"{"Output dimensions:",20} {input.ToString()}");

                // Save intermediate image if requested
                if (saveInter)
                {
                    input.Write(imageName);
                    // Show the saved name of inermediate image
                    if (logging)
                    {
                        Console.WriteLine($"{"Save as:",20} {imageName}.png");
                        // Convert from Byte to MB
                        double size = new FileInfo($"{imageName}.png").Length / 1000000.0;
                        Console.WriteLine($"{"Output size:",20} {size:F3} MB");
                    }
                }
                // A blank line
                if (logging) Console.WriteLine();
                
            }
            // Announce and return image
            Console.WriteLine("FINISHED PROCESSING NODES !");
            return input;
        }

        /// <summary>
        /// Convert a text input to a list of nodes
        /// </summary>
        /// <param name="txtFile"> Text file </param>
        /// <returns> Return a list of nodes </returns>
        public static List<Node> Load(string txtFile) 
        {
            List<Node> pipeline = new List<Node> () {};
            // Read text file
            string[] line = File.ReadAllLines(txtFile);
            for (int i = 0; i < line.Length; i++)
            {
                // If node has no other parameter (tell based on the number of "=" sign)
                if (line[i].Length - line[i].Replace("=","").Length == 1)
                    pipeline.Add(ParseNodeString(line[i]));
                // If node has other parameters
                else pipeline.Add(ParseNodeKey(line[i]));
            }
            return pipeline;
        }

        /// <summary>
        /// Parse a string with no parameters to Node
        /// </summary>
        /// <param name="line"> Input string to parse </param>
        /// <returns> Return a Node </returns>
        static Node ParseNodeString(string line)
        {
            // After remove key words and only leave the name of node, parse the string to the according Node
            switch (line.Remove(0,5).Trim())
            {
                case "greyscale":
                    return new N_GreyScale();
                case "normalise":
                    return new N_Normalise();
                default:
                    return new N_Vignette();
            }
        }

        /// <summary>
        /// Parse a string with parameters to Node
        /// </summary>
        /// <param name="line"> Input string to parse </param>
        /// <returns> Return a Node </returns>
        static Node ParseNodeKey(string line)
        {
            // Split string into separate element
            string[] lineArr = line.Split(" ");
            // After remove key words and only leave the name and relevant values, parse the string to the according Node
            switch (lineArr[0].Remove(0,5))
            {
                case "noise":
                    double a = double.Parse(lineArr[1].Remove(0,13));
                    return new N_Noise(a);
                default:
                    return new N_Crop(ParseVec(lineArr[1].Remove(0,7))[0],
                                      ParseVec(lineArr[1].Remove(0,7))[1],
                                      ParseVec(lineArr[2].Remove(0,5))[0],
                                      ParseVec(lineArr[2].Remove(0,5))[1]);
            }
        }
        /// <summary>
        /// Parse a string to a vector
        /// </summary>
        /// <param name="vecString"> Input string to parse </param>
        /// <returns> Return a vector </returns>
        static int[] ParseVec(string vecString) 
        {
            string[] vectorArr = vecString.Split("x");
            int[] vector = new int[] {
                int.Parse(vectorArr[0]),
                int.Parse(vectorArr[1])
            };
            return vector;
        }

    }

    /// <summary>
    /// Change image to gray scale
    /// </summary>
    class N_GreyScale : Node
    {
        public override string Name() { return "GreyScale"; } 

        public override Image Process(Image input)
        {
            input = Image.ToGrayscale(input);
            return input;
        }
    }

    /// <summary>
    /// Crop initial image to certain width and height at an assigned position
    /// </summary>
    class N_Crop : Node
    {
        // Private fields
        private int _x;
        private int _y;
        private int _widthCrop;
        private int _heightCrop;

        // Constructors: initialises data members
        public N_Crop(int x, int y, int widthCrop, int heightCrop)
        {
            _x = x;
            _y = y;
            _widthCrop = widthCrop;
            _heightCrop = heightCrop;
        }

        public override string Name() { return "Crop"; } 
        public override string OtherInfo() { return $"(origin=({_x},{_y}), size=({_widthCrop},{_heightCrop}))"; }
        // Method
        public override Image Process(Image input)
        {
            // Create a blank image of the output size
            Image output = new Image(_widthCrop,_heightCrop);
            
            // Copy each pixel of the crop area to "output"
            for (int x = 0; x < _widthCrop; x++)
            {
                for (int y = 0; y < _heightCrop; y++)
                {
                    Rgba32 value = input.GetPixel(x + _x, y + _y);
                    output.SetPixel(x,y, value);
                }
            }
            return output;
        }
    }

    /// <summary>
    /// Intensify the constrast of the image
    /// </summary>
    class N_Normalise : Node
    {
        public override string Name() { return "Normalise"; } 

        public override Image Process(Image input)
        {
            input = Image.ToGrayscale(input);

            double oldMax = 0;
            double oldMin = 255;
            double relativeBrightness;
            double newNum;
            
            // Find the max (whitest) and min (blackest) value in the old image
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    Rgba32 oldValue = input.GetPixel(x,y);
                    oldMax = Math.Max(oldMax, oldValue.R);
                    oldMin = Math.Min(oldMin, oldValue.R);
                }
            }

            // Iterate throught each pixel and change it to new value according to its relativeBrightness
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    Rgba32 oldValue = input.GetPixel(x,y);
                    // A bit of maths
                    relativeBrightness = (oldValue.R - oldMin) / (oldMax - oldMin);
                    newNum = relativeBrightness * 255;
                    // Create new pixel value
                    Rgba32 newValue = new Rgba32(
                        r: (byte)(newNum),
                        g: (byte)(newNum),
                        b: (byte)(newNum)
                    );
                    // And Set new value into the old one
                    input.SetPixel(x, y, newValue);
                }
            }

            return input;
        }
    }
    
    /// <summary>
    /// Reduce an image's brightness towards the periphery compared to the image's center
    /// </summary>
    class N_Vignette : Node
    {
        public override string Name() { return "Vigenette"; } 

        public override Image Process(Image input)
        {   
            double centerX = input.Width / 2;
            double centerY = input.Height / 2;
            double maxDis = (int)(Math.Sqrt( centerX * centerX + centerY * centerY));
            
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    // Calculate the brightness value
                    double distance = (int)(Math.Sqrt( (centerX-x)*(centerX-x) + (centerY-y)*(centerY-y) ));
                    double brightness = (maxDis - distance) / maxDis;
                    brightness *= brightness;
                    // Get, Process, and Set
                    Rgba32 oldValue = input.GetPixel(x,y);
                    Rgba32 newValue = new Rgba32(
                        r: (byte)(oldValue.R * brightness),
                        g: (byte)(oldValue.G * brightness),
                        b: (byte)(oldValue.B * brightness)
                    );
                    input.SetPixel(x, y, newValue);
                }
            }

            return input;
        }
    }
    
    /// <summary>
    /// Add or reduce brightness randomly of each pixel in an image
    /// </summary>
    class N_Noise : Node
    { 
        public override string Name() { return "Noise"; } 
        public override string OtherInfo() { return $"(noiseValue={_noiseValue})"; }
        // Private field
        private double _noiseValue;

        // Setter
        public N_Noise(double noiseValue)
        {
            // if (noiseValue > 1 || noiseValue < 0)
            //     throw new Exception("Invalid input! noiseValue must be from 0 to 1");
            _noiseValue = noiseValue;
        }

        public override Image Process(Image input)
        {
            // Iterate throught each pixel and make noise to it 
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    // Create a random number whose range relies on "_noiseValue"
                    Random r = new Random();
                    double randomNum = r.Next( (int)(-255*_noiseValue), (int)(255*_noiseValue) );

                    // Get, Process, and Set
                    Rgba32 oldValue = input.GetPixel(x,y);
                    Rgba32 newValue = new Rgba32(
                        r: (byte)(Math.Min(255, Math.Max(0, oldValue.R + randomNum))),
                        g: (byte)(Math.Min(255, Math.Max(0, oldValue.G + randomNum))),
                        b: (byte)(Math.Min(255, Math.Max(0, oldValue.B + randomNum)))
                    );
                    input.SetPixel(x, y, newValue);
                }
            }
            return input;
        }
    }
}
