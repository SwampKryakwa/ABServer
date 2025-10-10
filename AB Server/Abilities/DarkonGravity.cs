using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class DarkonGravity : AbilityCard
{
    public DarkonGravity(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = x => x.Position != User.Position && x.OnField() }
        ];
    }

    public override void TriggerEffect()
    {
        if (User.Position is GateCard posGate)
            (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan?.Move(posGate, new JObject { ["MoveEffect"] = "LightningChain", ["Attribute"] = (int)User.BaseAttribute, ["EffectSource"] = User.BID });
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Darkon);

    public override bool ActivationCondition() =>
        Owner.Bakugans.Count == 0;

    [ModuleInitializer]
    internal static void Init() => Register(28, CardKind.NormalAbility, (cID, owner) => new DarkonGravity(cID, owner, 28));
}
