using Code.Game.Input.Service;
using Entitas;

namespace Code.Game.Features.Input.Systems
{
    public class InitializeInputSystem : IInitializeSystem
    {
        private readonly IInputService _inputService;
        private readonly InputContext _inputContext;

        public InitializeInputSystem(IInputService inputService)
        {
            _inputService = inputService;
            _inputContext = Contexts.sharedInstance.input;
        }

        public void Initialize()
        {
            _inputService.EnableInput();

            var pointer = _inputContext.CreateEntity();
            pointer.isInput = true;
            pointer.isPointerState = true;
            pointer.AddPointerInput(_inputService.GetPointer());
            pointer.AddWorldPointerInput(_inputService.GetWorldPointer());
        }
    }
}
