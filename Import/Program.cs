﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Newtonsoft.Json;

namespace Import
{
    class Program
    {
        private static readonly int BATCH_SIZE = 50;

        static void Main(string[] args)
        {
            // Create the Api, passing in the training key
            string trainingKey = "5b77bf58cea64973b375fbc516da5f1a";
            if (String.IsNullOrEmpty(trainingKey)) 
            {
                Console.WriteLine("The custom vision training key needs to be set.");
                Environment.Exit(1);
            }

            CustomVisionTrainingClient trainingApi = new CustomVisionTrainingClient(new ApiKeyServiceClientCredentials(trainingKey)) { Endpoint = "https://sketch2html-cogservice.cognitiveservices.azure.com/" };

            // Find the object detection project
            var domains = trainingApi.GetDomains();
            var objDetectionDomain = domains.FirstOrDefault(d => d.Type == "ObjectDetection");
            var project = trainingApi.GetProjects().FirstOrDefault(p => p.Settings.DomainId == objDetectionDomain.Id);

            string modelPath = @"C:\Users\dsahithirani\Desktop\Sketch2Code-master\model"; //Path.Combine(Environment.CurrentDirectory, "..", "model");
            string dataPath = Path.Combine(modelPath, "dataset.json");
            string imagesPath = Path.Combine(modelPath, "images");

          //  var projectId = new Guid("fa6d6d1c-a9ef-4d92-9125-484a68f80dd5");
            var images = JsonConvert.DeserializeObject<IEnumerable<Image>>(File.ReadAllText(dataPath));

            // Create tags, unless they already exist
            var existingTags = trainingApi.GetTags(project.Id);
            var tagsToImport = images
                .SelectMany(i => i.Tags
                    .Select(t => t.TagName))
                .Distinct()
                .Where(t => !existingTags.Any(e => string.Compare(e.Name, t, ignoreCase: true) == 0));
            if (tagsToImport.Any()) 
            {
                foreach (var tag in tagsToImport) 
                {
                    Console.WriteLine($"Importing {tag}");
                    var newTag = trainingApi.CreateTag(project.Id, tag);
                    existingTags.Add(newTag);
                }
            }

            // Upload images with region data, in batches of 50
            while (images.Any()) 
            {
                var currentBatch = images.Take(BATCH_SIZE);

                var imageFileEntries = new List<ImageFileCreateEntry>();
                foreach (var imageRef in currentBatch) 
                {
                    var regions = new List<Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.Region>();
                    foreach (var region in imageRef.Regions) {
                        regions.Add(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models.Region() {
                            Height = region.Height,
                            Width = region.Width,
                            Top = region.Top,
                            Left = region.Left,
                            TagId = existingTags.First(t => t.Name == region.TagName).Id
                        });
                    }
                    
                    string imagePath = Path.Combine(imagesPath, string.Concat(imageRef.Id, ".png"));
                    imageFileEntries.Add(new ImageFileCreateEntry() 
                    {
                        Name = imagePath,
                        Contents = File.ReadAllBytes(imagePath),
                        Regions = regions
                    });
                }

                trainingApi.CreateImagesFromFiles(project.Id, new ImageFileCreateBatch(imageFileEntries));

                images = images.Skip(BATCH_SIZE);
            }

            Console.WriteLine("Training data upload complete!");
        }
    }
}
