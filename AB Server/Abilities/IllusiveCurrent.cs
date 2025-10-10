using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class IllusiveCurrent : AbilityCard
{
    public IllusiveCurrent(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner && x.InHand() }
        ];
    }

    GateCard oldUserPos;
    public override void Resolve()
    {
        if (counterNegated)
        {
            Dispose();
            Game.ChainStep();
            return;
        }
        if (User.OnField())
        {
            oldUserPos = (User.Position as GateCard)!;
            User.MoveFromFieldToHand(oldUserPos.EnterOrder);
            base.Resolve();
        }
        else if (User.InDrop())
        {
            User.MoveFromDropToHand();
            Dispose();
            Game.ChainStep();
        }
        else
        {
            Dispose();
            Game.ChainStep();
        }
    }

    public override void TriggerEffect() =>
        (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.AddFromHandToField(oldUserPos);

    public override bool UserValidator(Bakugan user) =>
        user.OnField();
    
    public override bool ActivationCondition() =>
        Owner.Bakugans.Any(x => x.IsAttribute(Attribute.Aqua));

    [ModuleInitializer]
    internal static void Init() => Register(13, CardKind.NormalAbility, (cID, owner) => new IllusiveCurrent(cID, owner, 13));
}

