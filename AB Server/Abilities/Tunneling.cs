using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Tunneling : AbilityCard
{
    public Tunneling(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = gate => gate.OnField && (gate != User.Position as GateCard) && gate.Position.X == (User.Position as GateCard)!.Position.X && gate != User.Position && !gate.IsAdjacent((User.Position as GateCard)!) }
        ];
    }

    public override void TriggerEffect() =>
        GenericEffects.MoveBakuganEffect(User, (CondTargetSelectors[0] as GateSelector)!.SelectedGate, new JObject() { ["MoveEffect"] = "Submerge" });

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Subterra) && user.Game.GateIndex.Any(gate => gate.OnField && (gate != user.Position as GateCard) && gate.Position.X == (user.Position as GateCard)!.Position.X && !gate.IsAdjacent((user.Position as GateCard)!));

    [ModuleInitializer]
    internal static void Init() => Register(10, CardKind.NormalAbility, (cID, owner) => new Tunneling(cID, owner, 10));
}
