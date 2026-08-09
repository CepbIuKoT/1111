namespace NorthernLands.Combat
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(DamageInfo damage);
    }
}
