using DG.Tweening;

namespace Code.Infrastructure.Tweens
{
    /// <summary>
    /// DOTween capacity reserved once at startup, so its internal arrays never grow during a battle.
    /// Views keep persistent tweens (created once, restarted), which hold a slot even while the view waits
    /// inactive in the pool.
    /// </summary>
    public static class TweenCapacity
    {
        public const int MinUnits = 500;

        // HitReactionView + HpBarView.
        public const int TweenersPerUnit = 2;

        // Unit views parked inactive in the view pool still own their tweens.
        public const int PooledUnitViews = 500;

        // One persistent tween per floating text view (pool keeps up to 200 per prefab).
        public const int FloatingTexts = 300;

        // Range view, UI and one-off tweens.
        public const int TweenersReserve = 200;
        public const int Sequences = 100;

        public const int Tweeners =
            (MinUnits * TweenersPerUnit) + (PooledUnitViews * TweenersPerUnit) + FloatingTexts + TweenersReserve;

        public static void Apply() => DOTween.SetTweensCapacity(Tweeners, Sequences);
    }
}
