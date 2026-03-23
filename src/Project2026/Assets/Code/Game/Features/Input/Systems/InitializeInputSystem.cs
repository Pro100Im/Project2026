using Code.Game.Input.Service;
using Entitas;

namespace Code.Game.Features.Input.Systems
{
    public class InitializeInputSystem : IInitializeSystem
    {
        private readonly IInputService _inputService;

        public InitializeInputSystem(IInputService inputService)
        {
            _inputService = inputService; 
        }

        public void Initialize()
        {
            _inputService.EnableInput();
        }
    }
}