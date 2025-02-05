using Microsoft.AspNetCore.Mvc;
using UserInputs.Model;
using UserInputs.Services;

namespace UserInputs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInputController : ControllerBase
    {
        private readonly IUserInputsService _userInputsService;

        public UserInputController(IUserInputsService userInputsService)
        {
            _userInputsService = userInputsService;
        }

        // POST: api/UserInput
        [HttpPost]
        public async Task<IActionResult> PostUserInput([FromBody] UserInput userInput)
        {
            await _userInputsService.StoreInput(userInput);
            return Ok("User input stored successfully.");
        }

        // GET: api/UserInput
        [HttpGet]
        public async Task<IActionResult> GetUserInput()
        {
            var storedInput = await _userInputsService.GetStoredInput();
            return Ok(storedInput);
        }
    }
}
