using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Diastrophism : AbilityCard
{
    public Diastrophism(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector { ClientType = "GF", Message = "INFO_ABILITY_GATETARGET", ForPlayer = (p) => p == Owner, TargetValidator = g => g.OnField && g.Owner == Owner }
        ];
    }

    public override bool UserValidator(Bakugan user) => user.IsAttribute(Attribute.Subterra) && user.OnField();

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as GateSelector)!.SelectedGate;
        target.MarkAsIfOwnerBattling = true;
        Action removeMark = () => { };
        removeMark = () =>
        {
            target.MarkAsIfOwnerBattling = false;
            Game.OnTurnEnd -= removeMark;
        };
        Game.OnTurnEnd += removeMark;
    }

    [ModuleInitializer]
    internal static void Init() => Register(54, CardKind.NormalAbility, (cID, owner) => new Diastrophism(cID, owner, 54));
}