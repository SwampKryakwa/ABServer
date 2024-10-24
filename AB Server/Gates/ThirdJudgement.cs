namespace AB_Server.Gates
{
    internal class ThirdJudgement : GateCard, IGateCard
    {
        public ThirdJudgement(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 1;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
            AllowAnyPlayers = false;

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;
        }

        public new void Open()
        {
            IsOpen = true;
            AllowAnyPlayers = true;
            if (Bakugans.Count < 3)
                Freeze(this);

            game.BakuganMoved += OnBakuganMove;
            game.BakuganThrown += OnBakuganStands;
            game.BakuganPlacedFromGrave += OnBakuganStands;
            game.BakuganReturned += OnBakuganLeaves;
            game.BakuganDestroyed += OnBakuganLeaves;
        }

        public new void DetermineWinner()
        {
            foreach (Bakugan b in Bakugans)
                b.InBattle = false;

            int winnerPower = Bakugans.Max(x => x.Power);

            if (!Bakugans.Any(x => x < winnerPower))
            {
                Draw();
                return;
            }

            int winner = Array.IndexOf(teamTotals, teamTotals.Max());

            foreach (Bakugan b in new List<Bakugan>(Bakugans))
                if (b.Power != winner)
                    b.Destroy(EnterOrder, MoveSource.Game);


            foreach (List<JObject> e in game.NewEvents)
                e.Add(new JObject
                {
                    { "Type", "BattleOver" },
                    { "IsDraw", false },
                    { "Victor", winner }
                });

            game.OnBattleOver(this, (ushort)winner);

            foreach (Bakugan b in new List<Bakugan>(Bakugans))
                    b.ToHand(EnterOrder);

            game.Field[Position.X, Position.Y] = null;

            (this as IGateCard).Remove();
        }

        public new void Remove()
        {
            IsOpen = false;
            AllowAnyPlayers = false;
            TryUnfreeze(this);

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;

            base.Remove();
        }

        public void OnBakuganMove(Bakugan target, BakuganContainer pos)
        {
            if (Bakugans.Count < 3)
                Freeze(this);
            else
                TryUnfreeze(this);
        }

        public void OnBakuganStands(Bakugan target, ushort owner, BakuganContainer pos)
        {
            if (Bakugans.Count < 3)
                Freeze(this);
            else
                TryUnfreeze(this);
        }

        public void OnBakuganLeaves(Bakugan target, ushort owner)
        {
            if (Bakugans.Count < 3)
                Freeze(this);
            else
                TryUnfreeze(this);
        }
    }
}
