using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class VicariousVictim : AbilityCard
{
    public VicariousVictim(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BG", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner && x.InDrop()}
        ];
    }

    public override void TriggerEffect()
    {
        var selectedBakugan = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (User.Position is GateCard positionGate)
            if (selectedBakugan.Position is BakuganDrop)
            {
                selectedBakugan.MoveFromDropToField(positionGate);
                User.MoveFromFieldToDrop(positionGate.EnterOrder);
            }
            else if (selectedBakugan.Position is GateCard targetPositionGate)
            {
                selectedBakugan.MoveOnField(positionGate, new JObject { ["MoveEffect"] = "Slide" });
                User.MoveOnField(targetPositionGate, new JObject { ["MoveEffect"] = "Slide" });
            }
            else if (selectedBakugan.Position is Player)
            {
                selectedBakugan.AddFromHandToField(positionGate);
                User.MoveFromFieldToHand(positionGate.EnterOrder);
            }
    }

    public override bool UserValidator(Bakugan user) => user.OnField() && user.Type == BakuganType.Griffon;

    [ModuleInitializer]
    internal static void Init() => Register(21, CardKind.NormalAbility, (cID, owner) => new VicariousVictim(cID, owner, 21));
}