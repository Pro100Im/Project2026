using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Input.Systems
{
    public class CleanUpInputSystem : ICleanupSystem
    {
        private readonly IGroup<InputEntity> _clicks;
        private readonly IGroup<InputEntity> _destructed;

        private readonly List<InputEntity> _clicksBuffer = new(16);
        private readonly List<InputEntity> _destructedBuffer = new(16);

        public CleanUpInputSystem()
        {
            var inputContext = Contexts.sharedInstance.input;

            _clicks = inputContext.GetGroup(InputMatcher.ClickInput);
            _destructed = inputContext.GetGroup(InputMatcher.Destructed);
        }

        public void Cleanup()
        {
            var clicks = _clicks.GetEntities(_clicksBuffer);

            for (var i = 0; i < clicks.Count; i++)
                clicks[i].Destroy();

            var destructed = _destructed.GetEntities(_destructedBuffer);

            for (var i = 0; i < destructed.Count; i++)
                destructed[i].Destroy();
        }
    }
}
