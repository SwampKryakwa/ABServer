using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Marionette : FusionAbility
{
    public Marionette(int cID, Player owner) : base(cID, owner, 6, typeof(SlingBlazer))
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = bakugan => bakugan.Owner != Owner && bakugan.OnField() }
        ];
        ResTargetSelectors =
        [
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = gate => gate.IsAdjacent((User.Position as GateCard)!) }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        var destination = (ResTargetSelectors[0] as GateSelector)!.SelectedGate;
        if (destination.OnField)
            GenericEffects.MoveBakuganEffect(target, destination, new JObject { ["MoveEffect"] = "LightningChain", ["Attribute"] = (int)User.BaseAttribute, ["EffectSource"] = User.BID });
    }

    public override bool UserValidator(Bakugan user) =>
        user.Type == BakuganType.Mantis && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(2, (cID, owner) => new Marionette(cID, owner));
}
