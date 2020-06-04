using System;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Project
{ 
    public static class Program
    {
        /// <summary> Main function </summary>
        /// <param name="args">
        /// args[0] : image filename
        /// args[1] : pipefile
        /// args[2] : choose to print logging             => (true/false) 
        /// args[3] : choose to save intermediate images  => (true/false)
        /// args[4] : desired output path
        /// </param>
        /// <note> Must input all the arguments above </note>
        /// <example> Example: dotnet run cat.png pipeline.txt true true testing </example>
        public static void Main(string[] args) 
        {
            Image input = new Image(args[0]);

            // Load pipeline text file into a List of nodes
            List<Node> pipeline = Pipeline.Load(args[1]); 
            // Run pipeline with some other parameter
            Image output = Pipeline.Run(input, 
                                        pipeline, 
                                        logging: bool.Parse(args[2]), 
                                        saveInter: bool.Parse(args[3]), 
                                        saveDir: args[4]);

            string outputName = "finalOutput";
            // Save processed image
            output.Write(outputName);
            // Announce
            Console.WriteLine($"Image is saved as {outputName}.png");
        }
    }
}