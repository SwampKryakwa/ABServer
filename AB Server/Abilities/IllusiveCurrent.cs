using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class IllusiveCurrent : AbilityCard
{
    public IllusiveCurrent(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner && x.InHand() },
            new GateSelector() { ClientType = "GF", ForPlayer = p => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.OnField }
        ];
    }

    public override void Resolve()
    {
        if (counterNegated)
        {
            Dispose();
            Game.ChainStep();
            return;
        }
        if (User.OnField())
            User.MoveFromFieldToHand((User.Position as GateCard)!.EnterOrder);
        else if (User.InDrop())
            User.MoveFromDropToHand();
        base.Resolve();
    }

    public override void TriggerEffect() =>
        (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.AddFromHandToField((ResTargetSelectors[1] as GateSelector)!.SelectedGate);

    public override bool UserValidator(Bakugan user) =>
        user.OnField();

    public override bool ActivationCondition() =>
        Owner.Bakugans.Any(x => x.IsAttribute(Attribute.Aqua));

    [ModuleInitializer]
    internal static void Init() => Register(13, CardKind.NormalAbility, (cID, owner) => new IllusiveCurrent(cID, owner, 13));
}

