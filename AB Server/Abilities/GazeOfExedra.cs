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
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (target.InHand())
        {
            target.MoveFromHandToDrop();
            User.Boost(200, this);
        }
        else if (target.OnField())
        {
            target.MoveFromFieldToDrop((target.Position as GateCard)!.EnterOrder);
            User.Boost(200, this);
        }
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(50, CardKind.NormalAbility, (cID, owner) => new GazeOfExedra(cID, owner, 50));
}

