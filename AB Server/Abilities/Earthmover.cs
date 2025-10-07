using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Earthmover : AbilityCard
{
    public Earthmover(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new MultiGateSelector() { ClientType = "MGF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = x => !x.Bakugans.Any(x=>x.Owner.TeamId != Owner.TeamId) }
        ];
    }

    public override void Activate()
    {
        (CondTargetSelectors[0] as MultiGateSelector)!.MaxNumber = Game.GateIndex.Count(x => x.OnField && x.Owner == Owner);
        base.Activate();
    }

    public override void TriggerEffect()
    {
        foreach (var target in (CondTargetSelectors[0] as MultiGateSelector)!.SelectedGates)
        {
            new List<Bakugan>(target.Bakugans).ForEach(x => x.MoveFromFieldToHand(target.EnterOrder));

            target.ToDrop();
        }
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Worm && user.OnField() && Game.GateIndex.Any(x => !x.Bakugans.Any(x => x.Owner != Owner));

    [ModuleInitializer]
    internal static void Init() => Register(35, CardKind.NormalAbility, (cID, owner) => new Earthmover(cID, owner, 35));
}
