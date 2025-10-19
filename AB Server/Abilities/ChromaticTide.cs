using System.Runtime.CompilerServices;
using AB_Server.Abilities.Correlations;

namespace AB_Server.Abilities;

internal class ChromaticTide(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        AbilityCard ability = new AdjacentCorrelation(Game.AbilityIndex.Count, Owner);
        Owner.AbilityHand.Add(ability);
        Game.AbilityIndex.Add(ability);
        Game.ThrowEvent(EventBuilder.AbilityAddedToHand(ability));
        ability = new DiagonalCorrelation(Game.AbilityIndex.Count, Owner);
        Owner.AbilityHand.Add(ability);
        Game.AbilityIndex.Add(ability);
        Game.ThrowEvent(EventBuilder.AbilityAddedToHand(ability));
        ability = new TripleNode(Game.AbilityIndex.Count, Owner);
        Owner.AbilityHand.Add(ability);
        Game.AbilityIndex.Add(ability);
        Game.ThrowEvent(EventBuilder.AbilityAddedToHand(ability));
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Aqua);

    [ModuleInitializer]
    internal static void Init() => Register(44, CardKind.NormalAbility, (cID, owner) => new ChromaticTide(cID, owner, 44));
}
