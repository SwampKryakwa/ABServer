using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Steadfast(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void Setup(bool asCounter)
    {
        this.asCounter = asCounter;
        Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
            EventBuilder.HandBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

        Game.OnAnswer[Owner.Id] = RecieveUser;
    }

    public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Subterra) && user.InHand() && Owner.UsedThrows == 0;

    public override void TriggerEffect() =>
        Owner.AllowedThrows = 0;

    [ModuleInitializer]
    internal static void Init() => Register(55, CardKind.NormalAbility, (cID, owner) => new Steadfast(cID, owner, 55));
}