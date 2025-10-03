using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class BlowAway : AbilityCard
{
    public BlowAway(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.IsOpponentOf(User) && target.Position == User.Position}
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (Game.GateIndex.Where(x => x.OnField).Count() == 1)
            target.MoveFromFieldToHand((target.Position as GateCard).EnterOrder);
        else
        {
            GateCard[] possibleDestinations = [.. Game.GateIndex.Where(x => x != User.Position && x.OnField)];
            GenericEffects.MoveBakuganEffect(target, possibleDestinations[new Random().Next(possibleDestinations.Length)]);
        }
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Zephyros) && Game.CurrentWindow == ActivationWindow.Normal;

    public static new bool HasValidTargets(Bakugan user) =>
        user.Position.Bakugans.Any(x => x.Owner != user.Owner);

    [ModuleInitializer]
    internal static void Init() => Register(32, CardKind.NormalAbility, (cID, owner) => new BlowAway(cID, owner, 32));
}
