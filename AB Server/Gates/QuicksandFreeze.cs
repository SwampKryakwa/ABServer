namespace AB_Server.Gates
{
    class QuicksandFreeze : GateCard
    {
        public QuicksandFreeze(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 12;

        public override void DetermineWinnerNormalBattle()
        {
            int[] teamTotals = new int[game.SideCount];
            for (int i = 0; i < game.PlayerCount; i++) teamTotals[i] = 0;
            foreach (var b in Bakugans)
            {
                teamTotals[b.Owner.SideID] += b.Power;
            }

            int winnerPower = teamTotals.Max();

            if (teamTotals.Count(x => x == winnerPower) == 1)
            {
                int winner = Array.IndexOf(teamTotals, teamTotals.Max());

                foreach (Bakugan b in new List<Bakugan>(Bakugans))
                    if (b.Owner.SideID != winner)
                    {
                        b.JustEndedBattle = true;
                        b.DestroyOnField(EnterOrder, MoveSource.Game);
                    }
            }
            else
            {
                foreach (Bakugan b in Bakugans)
                {
                    b.BattleEndedInDraw = true;
                }
            }

            if (!IsOpen && !Negated)
                Open();
        }

        public override void FakeBattleNormal(int winnerPower)
        {
            foreach (Bakugan b in new List<Bakugan>(Bakugans.Where(x => x.Power < winnerPower)))
                b.ToHand(EnterOrder);

            if (!IsOpen && !Negated)
                Open();
        }

        public override void Open()
        {
            IsOpen = true;
            resolved = false;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, Bakugans)
            ));

            game.OnAnswer[Owner.Id] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            resolved = false;
            foreach (Bakugan b in Bakugans)
            {
                b.JustEndedBattle = true;
            }
            ActiveBattle = false;

            var numSides = Bakugans.Select(x => x.Owner.SideID).Distinct().Count();
            BattleOver = true;

            game.BattlesToEnd.Add(this);
        }

        bool resolved;

        public override void Dispose()
        {
            if (resolved || !IsOpen || Negated)
                base.Dispose();
            else
            {
                resolved = true;
                if (!CheckBattles())
                {
                    foreach (Bakugan b in new List<Bakugan>(Bakugans))
                    {
                        b.JustEndedBattle = false;
                        if (b == target) continue;
                        b.ToHand(EnterOrder);
                    }
                }
                else game.NextStep();
            }
        }

        public override bool IsOpenable() =>
            false;
    }
}
