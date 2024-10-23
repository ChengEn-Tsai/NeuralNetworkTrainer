using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DotNetAssignment2;
using DotNetAssignment2.Data;
using DotNetAssignment2.Models;
using Microsoft.VisualBasic;

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
            // TODO: Training part
            Train(form);
            // save in db
            _dbContext.TrainingForms.Add(form);
            await _dbContext.SaveChangesAsync();
            return Ok(new { Message = "Training form saved successfully!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"~~~~~~~~~~~~~Error: {ex.Message}");
            return StatusCode(500, new { Message = "Error saving form data.", Error = ex.Message });
        }
    }

    private async Task Train(TrainingForm form)
    {
        //TODO: GO TRAINING
        bool isClassification = form.TypeOfTraining == "classification";

        CsvNeuralNetwork nn = new CsvNeuralNetwork(form.Layers.ToArray<string>(), form.Epoch,
            batchSize: form.BatchSize,
            isClassification: isClassification,
            form.Label, 
            pathToDataset: form.FilePath, 
            form.FeatureSelector);
        nn.LoadDataset();
        nn.BuildModel();
        nn.TrainModel();
    }
}
