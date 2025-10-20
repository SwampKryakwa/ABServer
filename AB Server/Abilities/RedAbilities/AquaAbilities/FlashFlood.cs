using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class FlashFlood(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        foreach (var bak in Game.BakuganIndex.Where(x => x.OnField() && x.Owner.TeamId != Owner.TeamId))
            bak.Boost(-300, this);

        if (Owner.Bakugans.Count == 0 && User.Position is GateCard posGate)
            User.MoveFromFieldToHand(posGate.EnterOrder);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Aqua);

    [ModuleInitializer]
    internal static void Init() => Register(46, CardKind.NormalAbility, (cID, owner) => new FlashFlood(cID, owner, 46));
}
