using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class GazeOfExedra(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
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

