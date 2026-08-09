using NorthernLands.Combat;
using NUnit.Framework;

namespace NorthernLands.Tests.EditMode
{
    public sealed class DamageCalculatorTests
    {
        [Test]
        public void ArmorNeverIncreasesDamage()
        {
            Assert.That(DamageCalculator.AfterArmor(100f, 50f), Is.LessThan(100f));
        }

        [Test]
        public void FullBlockReductionCanPreventDamage()
        {
            Assert.That(DamageCalculator.AfterBlock(25f, true, 1f), Is.Zero);
        }

        [Test]
        public void NegativeInputsAreClamped()
        {
            Assert.That(DamageCalculator.AfterArmor(-10f, -5f), Is.Zero);
        }
    }
}
