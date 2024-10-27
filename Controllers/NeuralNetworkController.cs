using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DotNetAssignment2;
using DotNetAssignment2.Data;
using DotNetAssignment2.Models;
using Tensorflow.Keras;
using System.IO.Compression;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private static INeuralNetwork? CurrentModel; // variable to hold the current model

    public NeuralNetworkController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // POST: start training and save the form in db
    [HttpPost]
    [Route("start")]
    public async Task<IActionResult> StartTraining([FromBody] TrainingForm form)
    {
        try
        {
            if (form == null)
            {
                return BadRequest("Invalid form data.");
            }

            ValueGetter.Reset();

            // start training asynchronously
            _ = Task.Run(() => Train(form));

            // save the form in the database
            _dbContext.TrainingForms.Add(form);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Training started successfully!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new { Message = "Error starting training.", Error = ex.Message });
        }
    }

    // GET: gets real-time training metrics
    [HttpGet]
    [Route("status")]
    public IActionResult GetTrainingMetrics()
    {
        var metrics = new
        {
            Loss = ValueGetter.Loss.LastOrDefault(),
            Accuracy = ValueGetter.Accuracy.LastOrDefault(),
            ValLoss = ValueGetter.ValLoss.LastOrDefault(),
            ValAccuracy = ValueGetter.ValAccuracy.LastOrDefault(),
            Epoch = ValueGetter.Loss.Count, 
            IsTrainingComplete = ValueGetter.IsTrainingComplete
        };

        return Ok(metrics);
    }

    // GET: retrieve features from the current model
    [HttpGet]
    [Route("features")]
    public IActionResult GetFeatures()
    {
        try
        {
            if (CurrentModel == null)
            {
                return NotFound("No model is currently loaded or trained.");
            }

            if (CurrentModel is CsvNeuralNetwork csvNN)
            {
                var features = csvNN.GetFeatures();
                return Ok(features);
            }
            else
            {
                return BadRequest("Current model does not support feature retrieval.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving features: {ex.Message}");
        }
    }

    // POST: handle prediction requests
    [HttpPost]
    [Route("predict")]
    public IActionResult Predict([FromBody] PredictionRequest request)
    {
        try
        {
            if (CurrentModel == null)
            {
                return NotFound("No model is currently loaded or trained.");
            }

            var featureList = CurrentModel.GetFeatures();

            if (request.Features == null || request.Features.Length != featureList.Count)
            {
                return BadRequest($"Expected {featureList.Count} features, but received {request.Features?.Length ?? 0}.");
            }

            // perform prediction
            CurrentModel.TestModel(request.Features);

            // get the prediction result
            string predictionResult = CurrentModel.GetLastPrediction();

            return Ok(new PredictionResponse { Prediction = predictionResult });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Prediction error: {ex.Message}");
        }
    }

    // POST: load a pretrained model
    [HttpPost]
    [Route("load-model")]
    public IActionResult LoadModel([FromBody] LoadModelRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ModelPath))
            {
                return BadRequest("Model path is required.");
            }

            // load model
            var nn = new CsvNeuralNetwork(
                layers: new List<ILayer>(),
                epochs: 0,
                batchSize: 0,
                isClassification: false, 
                targetClass: "", 
                pathToDataset: "",
                featuresToKeep: ""
            );

            nn.LoadModel(request.ModelPath);

            // set loaded model as current model
            CurrentModel = nn;

            return Ok(new { Message = "Model loaded successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error loading model: {ex.Message}");
        }
    }

    public class PredictionRequest
    {
        public float[] Features { get; set; }
    }

    public class PredictionResponse
    {
        public string Prediction { get; set; }
    }

    public class LoadModelRequest
    {
        public string ModelPath { get; set; }
    }


    [HttpGet("download-model")]
    public IActionResult DownloadModel()
    {
        if (CurrentModel == null)
        {
            return NotFound("No model is currently loaded or trained.");
        }

        try
        {
            string modelName = "trained_model";
            string saveDirectory = Path.Combine("SavedModels", modelName);
            string zipPath = Path.Combine("SavedModels", $"{modelName}.zip");

            // check if model directory exists
            if (!Directory.Exists(saveDirectory))
            {
                return NotFound("Model directory does not exist after saving.");
            }

            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(saveDirectory, zipPath);

            var fileBytes = System.IO.File.ReadAllBytes(zipPath);

            // returns the file as a downloadable response
            return File(fileBytes, "application/zip", $"{modelName}.zip");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading model: {ex.Message}");
            return StatusCode(500, $"Error downloading model: {ex.Message}");
        }
    }



    private void Train(TrainingForm form)
    {
        try
        {
            bool isClassification = form.TypeOfTraining == "classification";

            var nn = new CsvNeuralNetwork(
                form.Layers.ToArray(),
                form.Epoch,
                batchSize: form.BatchSize,
                isClassification: isClassification,
                targetClass: form.Label,
                pathToDataset: form.FilePath,
                featuresToKeep: form.FeatureSelector
            );

            nn.LoadDataset();
            nn.BuildModel();
            nn.TrainModel();

            CurrentModel = nn;

            CurrentModel.SaveModel("trained_model");

            ValueGetter.IsTrainingComplete = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Training failed: {ex.Message}");
            ValueGetter.IsTrainingComplete = true;
        }
    }
}
