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

        public override bool CheckBattles()
        {
            if (IsFrozen || BattleOver) return false;

            bool isBattle = Bakugans.Count > 1;

            if (isBattle)
            {
                if (!ActiveBattle)
                {
                    game.BattlesToStart.Add(this);
                    Open();
                }
            }
            else
            {
                ActiveBattle = false;
            }

            return isBattle;
        }

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));

            if (Bakugans.Any(x => x.Owner == Owner && x.IsAttribute(Attribute.Darkon) && x.BasePower <= 370))
            {
                game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
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
            if ((bool)game.IncomingSelection[Owner.Id]["array"][0]["answer"])
            {
                game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
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
            target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
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
