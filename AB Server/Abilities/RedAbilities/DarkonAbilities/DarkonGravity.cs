using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class DarkonGravity : AbilityCard
{
    public DarkonGravity(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = x => x.Position != User.Position && x.OnField() }
        ];
    }

    public override void TriggerEffect()
    {
        if (User.Position is GateCard posGate)
            (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan?.MoveOnField(posGate, new JObject { ["MoveEffect"] = "LightningChain", ["Attribute"] = (int)User.BaseAttribute, ["EffectSource"] = User.BID });
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Darkon);

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Normal && Owner.Bakugans.Count == 0;

    [ModuleInitializer]
    internal static void Init() => Register(28, CardKind.NormalAbility, (cID, owner) => new DarkonGravity(cID, owner, 28));
}
