using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class GazeOfExedra : AbilityCard
{
    public GazeOfExedra(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.IsAttribute(Attribute.Darkon) && x.InHand() && x.Owner == Owner }
        ];
    }

    public override void TriggerEffect()
    {
        if (User.InHand())
            User.MoveFromHandToDrop();
        else if (User.OnField())
            User.MoveFromFieldToDrop((User.Position as GateCard)!.EnterOrder);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() || user.InHand();

    [ModuleInitializer]
    internal static void Init() => Register(50, CardKind.NormalAbility, (cID, owner) => new GazeOfExedra(cID, owner, 50));
}

