using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Groundshift : AbilityCard
{
    /*
     * Groundshift
     * REQUIREMENT: Choose your SUBTERRA bakugan on the field to use.
     * EFFECT: Target 1 gate card adjacent to the one user is on. Swaps it with user gate card. Bakugan remain in the same field sectors. 
     */
    public Groundshift(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new GateSelector { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATION", TargetValidator = g => User.Position is GateCard posGate && g.IsAdjacent(posGate) }
        ];
    }

    public override void TriggerEffect()
    {
        if (CondTargetSelectors[0] is GateSelector gateSelector && User.Position is GateCard posGate)
        {
            GateCard targetGate = gateSelector.SelectedGate;
            GateCard userGate = posGate;

            (userGate.Position, targetGate.Position) = (targetGate.Position, userGate.Position);
            (userGate.Bakugans, targetGate.Bakugans) = (targetGate.Bakugans, userGate.Bakugans);
            (userGate.EnterOrder, targetGate.EnterOrder) = (targetGate.EnterOrder, userGate.EnterOrder);

            foreach (var b in userGate.Bakugans)
                b.Position = userGate;

            foreach (var b in targetGate.Bakugans)
                b.Position = targetGate;

            Game.ThrowEvent(new()
            {
                ["Type"] = "GatesSwappedNotBakugans",
                ["Owner"] = Owner.PlayerId,
                ["Gate1"] = new JObject
                {
                    ["ID"] = userGate.CardId,
                    ["PositionX"] = targetGate.Position.X,
                    ["PositionY"] = targetGate.Position.Y
                },
                ["Gate2"] = new JObject
                {
                    ["ID"] = targetGate.CardId,
                    ["PositionX"] = userGate.Position.X,
                    ["PositionY"] = userGate.Position.Y
                }
            });
        }
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Subterra);

    [ModuleInitializer]
    internal static void Init() =>
        Register(15, CardKind.NormalAbility, (cID, owner) => new Groundshift(cID, owner, 15));
}
