namespace AB_Server.Gates
{
    internal class JokersWild : GateCard
    {
        public JokersWild(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 5;

        public override void Set(byte posX, byte posY)
        {
            game.BakuganAdded += CheckAutoConditions;
            base.Set(posX, posY);
        }

        public override void Dispose()
        {
            game.BakuganAdded -= CheckAutoConditions;
            base.Dispose();
        }

        public override void CheckAutoConditions(Bakugan target, byte owner, IBakuganContainer pos)
        {
            if (pos != this || IsOpen || Negated) return;

            if (Bakugans.Count >= 2)
                game.AutoGatesToOpen.Add(this);
        }

        public override void Open()
        {
            IsOpen = true;
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            if (Bakugans.Any(x => x.Owner == Owner && x.IsAttribute(Attribute.Darkon) && x.BasePower <= 370))
            {
                game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.BoolSelectionEvent("INFO_WANTTARGET")
                ));

                game.OnAnswer[Owner.Id] = Setup;
            }
            else
            {
                game.NextStep();
            }
        }

        public void Setup()
        {
            if ((bool)game.PlayerAnswers[Owner.Id]["array"][0]["answer"])
            {
                game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans.Where(x => x.Owner == Owner && x.IsAttribute(Attribute.Darkon) && x.BasePower <= 370))
                ));
                game.OnAnswer[Owner.Id] = PickTarget;
            }
            else
            {
                game.NextStep();
            }
        }

        Bakugan? target;

        public void PickTarget()
        {
            target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]["array"][0]["bakugan"]];

            game.NextStep();
        }

        public override void Resolve()
        {
            if (target is not null)
                DetermineWinner();
        }

        public override bool IsOpenable() =>
            false;
    }
}
