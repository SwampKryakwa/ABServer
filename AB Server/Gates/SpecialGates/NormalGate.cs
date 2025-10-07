using AB_Server.Abilities;

namespace AB_Server.Gates.SpecialGates;

internal class NormalGate(int cID, Player owner) : GateCard(cID, owner)
{

    public override int TypeId { get; } = 0;
    public override CardKind Kind { get; } = CardKind.SpecialGate;

    public override void Open()
    {
        IsOpen = true;
        Game.ActiveZone.Add(this);
        Game.CardChain.Push(this);
        EffectId = Game.NextEffectId++;
        Game.ThrowEvent(EventBuilder.GateOpen(this));

        Game.CheckChain(Owner, this);
    }

    public override void Resolve()
    {
        Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
            EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, 4, Bakugans.Where(x => x.Owner == Owner))
        ));

        Game.OnAnswer[Owner.Id] = Activate;
    }

    public void Activate()
    {
        Bakugan target = Game.BakuganIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];

        if (!Negated && target.Position == this)
            target.Boost(new Boost((short)(new Random().Next(1, 10) * 10)), this);

        Game.ChainStep();
    }
}
