using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
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
                Console.WriteLine("===============NULL FORM!!!!!!!!!!");
                return BadRequest("Invalid form data.");
            }
            Console.WriteLine("===============1111111111!!!!!!!!!!");
            // TODO: Training part
            Train(form);
            Console.WriteLine("===============22222222222!!!!!!!!!!");
            // save in db
            _dbContext.TrainingForms.Add(form);
            Console.WriteLine("===============3333333333!!!!!!!!!!");
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("===============44444444444!!!!!!!!!!");
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
        //TODO: GO TRAINING!!!
    }
}
