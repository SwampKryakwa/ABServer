using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class SpiritCanyon(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        User.Boost(Game.GateIndex.Count(x => x.OnField && x.Owner == Owner) * 50, this);
        if (User.Position is GateCard posGate && posGate.Owner.TeamId != Owner.TeamId && posGate.IsOpen)
            User.Boost(Game.GateIndex.Count(x => x.OnField && x.Owner.TeamId != Owner.TeamId) * 50, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Subterra);

    [ModuleInitializer]
    internal static void Init() => Register(1, CardKind.NormalAbility, (cID, owner) => new SpiritCanyon(cID, owner, 1));
}
