using UserInputs.Model;

namespace UserInputs.Services
{
    public interface IUserInputsService
    {
        Task StoreInput(UserInput input);
        Task<UserInput> GetStoredInput();
    }
}
