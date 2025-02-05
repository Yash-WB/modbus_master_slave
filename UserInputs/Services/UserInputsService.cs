using UserInputs.Model;

namespace UserInputs.Services
{
    public class UserInputsService : IUserInputsService
    {
        private int _startAddress;
        private int _numRegisters;

        public Task StoreInput(UserInput input)
        {
            _startAddress = input.StartAddress;
            _numRegisters = input.NumRegisters;

            return Task.CompletedTask;
        }

        public Task<UserInput> GetStoredInput()
        {
            var storedInput = new UserInput
            {
                StartAddress = _startAddress,
                NumRegisters = _numRegisters
            };

            return Task.FromResult(storedInput);
        }
    }
}
