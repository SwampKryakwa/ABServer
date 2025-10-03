using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Aftershock : AbilityCard
{
    public Aftershock(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector { ClientType = "GF", Message = "INFO_ABILITY_GATETARGET", ForPlayer = (p) => p == Owner, TargetValidator = g => g.OnField && g.Owner == Owner }
        ];
    }

    public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Subterra) && user.OnField();

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as GateSelector)!.SelectedGate;
        target.MarkAsIfOwnerBattling = true;
        Game.TurnEndEffect removeMark = () => { };
        removeMark = () =>
        {
            target.MarkAsIfOwnerBattling = false;
            Game.TurnEnd -= removeMark;
        };
        Game.TurnEnd += removeMark;
    }

    [ModuleInitializer]
    internal static void Init() => Register(54, CardKind.NormalAbility, (cID, owner) => new Aftershock(cID, owner, 54));
}