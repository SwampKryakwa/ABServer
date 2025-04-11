 namespace AB_Server.Gates
{
    internal class Reloaded : GateCard
    {
        public Reloaded(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            CardId = cID;
        }

        public override int TypeId { get; } = 10;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans.Where(x => x.Owner == Owner))
            ));

            game.OnAnswer[Owner.Id] = Setup1;
        }

        Bakugan target1;
        Bakugan target2;

        public void Setup1()
        {
            target1 = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];


            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, game.BakuganIndex.Where(x => x.Owner == Owner && x.Position != this && x.OnField()))
            ));

            game.OnAnswer[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            target2 = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!counterNegated)
            {
                if (target1.OnField())
                {
                    target1.Boost(new Boost(100), this);
                }
                if (target2.OnField())
                {
                    target2.Boost(new Boost(-100), this);
                }
            }
        }

        public override bool IsOpenable() =>
            !IsOpen && Bakugans.Any(x => x.Owner == Owner) && Bakugans.Count >= 2 && game.GateIndex.Count(x => x.Bakugans.Any(y => y.Owner == Owner)) >= 2;
    }
}
