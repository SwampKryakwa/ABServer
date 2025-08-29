using AB_Server.Gates;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace AB_Server.Abilities.Fusions
{
    internal class Tremors(int cID, Player owner) : FusionAbility(cID, owner, 5, typeof(NoseSlap))
    {
        public override void TriggerEffect()
        {
            foreach (var target in Game.GateIndex.Where((User.Position as GateCard)!.IsDiagonal))
                foreach (var bakugan in target.Bakugans)
                    bakugan.Boost(-bakugan.Power, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Elephant && user.Position is GateCard userPos && Game.GateIndex.Any(userPos.IsDiagonal);

        [ModuleInitializer]
        internal static void Init() => Register(6, (cID, owner) => new Tremors(cID, owner));
    }
}
