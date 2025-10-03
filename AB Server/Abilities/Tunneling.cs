using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Tunneling : AbilityCard
{
    /*
        REQUIREMENT: Used by a standing SUBTERRA bakugan. Target 1 other bakugan on the same horizontal or vertical line as user, but not standing on adjacent gate card and 1 gate card in between the two. 
        EFFECT: Move user and the target on that gate card. 
    */
    public Tunneling(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = b => b.Position is GateCard targetPosGate && User.Position is GateCard userPosGate && targetPosGate != userPosGate && Game.GateIndex.Any(x=>x.IsBetween(userPosGate, targetPosGate)) },
            new GateSelector { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATION", TargetValidator = g => User.Position is GateCard userPosGate &&(CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Position is GateCard targetPosGate && g.IsBetween(userPosGate, targetPosGate) }
        ];
    }

    public override void TriggerEffect()
    {
        if (CondTargetSelectors[0] is BakuganSelector targetSelector && CondTargetSelectors[1] is GateSelector gateSelector)
        {
            var target = targetSelector.SelectedBakugan;
            if (!User.OnField() || !target.OnField()) return;

            var gate = gateSelector.SelectedGate;
            if (target != null && gate != null)
            {
                User.Move(gate, new() { ["MoveEffect"] = "Submerge" });
                target.Move(gate, new() { ["MoveEffect"] = "Submerge" });
            }
        }
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Subterra) && Game.BakuganIndex.Any(b => b.Position is GateCard targetPosGate && User.Position is GateCard userPosGate && targetPosGate != userPosGate && Game.GateIndex.Any(x => x.IsBetween(userPosGate, targetPosGate)));

    [ModuleInitializer]
    internal static void Init() => Register(10, CardKind.NormalAbility, (cID, owner) => new Tunneling(cID, owner, 10));
}
