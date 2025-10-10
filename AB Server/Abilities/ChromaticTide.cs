using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class ChromaticTide : AbilityCard
{
    public ChromaticTide(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new AbilitySelector() { ClientType = "A", ForPlayer = x => x == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Owner == Owner && x.Kind == CardKind.CorrelationAbility }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as AbilitySelector)!.SelectedAbility;

        target.FromDropToHand();

        var currentState = User.attributeChanges.Count != 0 ? User.attributeChanges[0].Attributes : [User.BaseAttribute];
        int sharedCount = 0;
        foreach (Bakugan bak in Owner.BakuganOwned)
        {
            var bakState = bak.attributeChanges.Count != 0 ? bak.attributeChanges[0].Attributes : [bak.BaseAttribute];
            foreach (var attr in currentState)
                if (bakState.Contains(attr))
                {
                    sharedCount++;
                    break;
                }
        }

        if (sharedCount == Owner.BakuganOwned.Count)
        {
            User.Boost(100, this);
        }
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Aqua);

    [ModuleInitializer]
    internal static void Init() => Register(44, CardKind.NormalAbility, (cID, owner) => new ChromaticTide(cID, owner, 44));
}
