namespace AB_Server.Gates
{
    internal class JokersWild : GateCard
    {
        public JokersWild(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 5;

        public override bool IsOpenable() =>
            false;

        public override void CheckAutoBattleStart()
        {
            Console.WriteLine("Checking if JW can be open");
            if (OpenBlocking.Count == 0 && !IsOpen && !Negated)
                game.AutoGatesToOpen.Add(this);
        }

        public override void Open()
        {
            IsOpen = true;
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            if (Bakugans.Any(x => x.Owner == Owner && x.IsAttribute(Attribute.Darkon) && x.BasePower <= 370))
            {
                game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                    EventBuilder.BoolSelectionEvent("INFO_WANTTARGET")
                ));

                game.OnAnswer[Owner.Id] = Setup;
            }
            else
            {
                game.CheckChain(Owner, this);
            }
        }

        public void Setup()
        {
            if ((bool)game.PlayerAnswers[Owner.Id]["array"][0]["answer"])
            {
                game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(false,
                    EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans.Where(x => x.Owner == Owner && x.IsAttribute(Attribute.Darkon) && x.BasePower <= 370))
                ));
                game.OnAnswer[Owner.Id] = PickTarget;
            }
            else
            {
                game.CheckChain(Owner, this);
            }
        }

        Bakugan? target;

        public void PickTarget()
        {
            target = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            Console.WriteLine("Is JW's target not null: " + target is not null);
            if (target is not null)
                DetermineWinner();

            game.ChainStep();
        }
    }
}
