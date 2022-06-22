﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.AI.Entities
{
    public class Project
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class PredictionResult
    {
        [JsonProperty(PropertyName = "predictions")]
        public IList<PredictionModel> Predictions { get; set; }
    }

    public class PredictionModel
    {
        [JsonProperty(PropertyName = "probability")]
        public double Probability { get; set; }

        [JsonProperty(PropertyName = "tagId")]
        public Guid TagId { get; set; }

        [JsonProperty(PropertyName = "tagName")]
        public string TagName { get; set; }

        [JsonProperty(PropertyName = "boundingBox")]
        public BoundingBox BoundingBox { get; set; }
    }

    public class BoundingBox
    {
        [JsonProperty(PropertyName = "left")]
        public double Left { get; set; }

        [JsonProperty(PropertyName = "top")]
        public double Top { get; set; }

        [JsonProperty(PropertyName = "width")]
        public double Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public double Height { get; set; }

    }

    public class Word
    {
        public string boundingBox { get; set; }
        public string text { get; set; }
    }

    public class Line
    {
        public string boundingBox { get; set; }
        public List<Word> words { get; set; }
    }

    public class Region
    {
        public string boundingBox { get; set; }
        public List<Line> lines { get; set; }
    }

    public class OCRData
    {
        public string language { get; set; }
        public double textAngle { get; set; }
        public string orientation { get; set; }
        public List<Region> regions { get; set; }
    }
}
