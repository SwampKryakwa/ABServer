namespace AB_Server.Abilities;

internal class Aftershock : AbilityCard
{
    public Aftershock(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors = [];
    }

    public override bool IsActivateableByBakugan(Bakugan user) => user.IsAttribute(Attribute.Subterra) && user.OnField();

    public override void TriggerEffect()
    {
    }
}