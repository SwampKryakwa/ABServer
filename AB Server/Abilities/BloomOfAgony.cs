using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class BloomOfAgony : AbilityCard
    {
        public BloomOfAgony(int cID, Player owner, int typeId) : base(cID, owner, typeId) { }

        public override void TriggerEffect()
        {
            foreach (Bakugan target in Game.BakuganIndex.Where(x => x.OnField()))
                target.Boost(-300, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) => user.Position is GateCard posGate && posGate.BattleStarting && user.IsAttribute(Attribute.Darkon);

        [ModuleInitializer]
        internal static void Init() => AbilityCard.Register(12, CardKind.NormalAbility, (cID, owner) => new BloomOfAgony(cID, owner, 12));
    }
}
