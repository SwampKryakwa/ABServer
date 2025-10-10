namespace AB_Server.Abilities;

internal class LightLine : AbilityCard
{
    public LightLine(int cID, Player owner, int typeId) : base(cID, owner, typeId) { }

    public override void TriggerEffect()
    {
        // Get all Haos Bakugan on the field owned by this card's owner
        var haosBakugan = Game.BakuganIndex
            .Where(b => b.OnField() && b.Owner == Owner && b.IsAttribute(Attribute.Lumina))
            .ToArray();

        var numMyBakuganNotUserOnField = Game.BakuganIndex.Where(b => b.OnField() && b.Owner == Owner && b != User).Count();

        foreach (var bak in haosBakugan)
            bak.Boost(100 * numMyBakuganNotUserOnField, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Lumina);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Init() =>
        Register(53, CardKind.NormalAbility, (cID, owner) => new LightLine(cID, owner, 53));
}