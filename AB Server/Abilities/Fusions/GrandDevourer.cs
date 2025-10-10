using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions;

internal class GrandDevourer : FusionAbility
{
    public GrandDevourer(int cID, Player owner) : base(cID, owner, 12, typeof(Earthmover))
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTROYTARGET", TargetValidator = x => x.OnField() && x.Owner != Owner }
        ];

        ResTargetSelectors =
        [
            new YesNoSelector() { ForPlayer = x => x == (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner, Message = "INFO_ABILITY_WANTDISCARD", Condition = () =>(CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner.AbilityHand.Count != 0, IsYes = false },
            new GateSelector() { ClientType = "GH", Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes, ForPlayer = x => x == (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner, Message = "Select a gate card to discard", TargetValidator = x => x.Owner == (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Owner }
        ];
    }

    public override void TriggerEffect()
    {
        if ((ResTargetSelectors[0] as YesNoSelector)!.IsYes)
        {
            (ResTargetSelectors[1] as GateSelector)!.SelectedGate.RemoveFromHand();
        }
        else
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (target.Position is GateCard posGate)
                target.MoveFromFieldToDrop(posGate.EnterOrder);
            else if (target.InHand())
                target.MoveFromHandToDrop();
        }
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        user.Type == BakuganType.Worm && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(11, (cID, owner) => new GrandDevourer(cID, owner));
}
