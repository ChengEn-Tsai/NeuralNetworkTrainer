using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DotNetAssignment2;
using DotNetAssignment2.Data;
using DotNetAssignment2.Models;

[Route("api/[controller]")]
[ApiController]
public class NeuralNetworkController : ControllerBase
{
    private readonly AppDbContext _dbContext;

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

            // Reset training state
            ValueGetter.Reset();

            // Start training asynchronously
            _ = Task.Run(() => Train(form));

            // Save the form in the database
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

    // GET: Get real-time training metrics
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
            Epoch = ValueGetter.Loss.Count, // Assuming Loss count is equivalent to epochs processed
            IsTrainingComplete = ValueGetter.IsTrainingComplete
        };

        return Ok(metrics);
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

            // Set training as complete
            ValueGetter.IsTrainingComplete = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Training failed: {ex.Message}");
            // Mark training as complete even if it failed
            ValueGetter.IsTrainingComplete = true;
        }
    }
}
