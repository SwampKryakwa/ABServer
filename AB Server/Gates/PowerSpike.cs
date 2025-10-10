namespace AB_Server.Gates;

internal class PowerSpike : GateCard
{
    public PowerSpike(int cID, Player owner) : base(cID, owner)
    {
        Game = owner.Game;
        Owner = owner;

        CardId = cID;

        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = x=> x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this }
        ];
    }

    public override int TypeId { get; } = 17;

    // Precompute the steps from -300 to +300 in increments of 50
    int[] steps = [.. Enumerable.Range(-6, 7).Select(i => i * 50)];
    public override void TriggerEffect()
    {
        Bakugan target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;

        // Randomly pick -300 to +300 (in steps of 50)
        target.Boost(steps[new Random().Next(steps.Length)], this);
    }
}
