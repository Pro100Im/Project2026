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

        // HitReactionView. HP bars are batched and animated by systems (HpBarBackFillSystem), no tween.
        public const int TweenersPerUnit = 1;

        // Unit views parked inactive in the view pool still own their tweens.
        public const int PooledUnitViews = 500;

        // Battle damage numbers are batched (FloatingTextRenderSystem), no tween. Only the Town still uses
        // FloatingTextView (one persistent tween per pooled view).
        public const int FloatingTexts = 64;

        // Range view, UI and one-off tweens.
        public const int TweenersReserve = 200;
        public const int Sequences = 100;

        public const int Tweeners =
            (MinUnits * TweenersPerUnit) + (PooledUnitViews * TweenersPerUnit) + FloatingTexts + TweenersReserve;

        public static void Apply() => DOTween.SetTweensCapacity(Tweeners, Sequences);
    }
}
