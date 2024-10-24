using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class QuicksandFreeze : GateCard, IGateCard
    {
        bool effectTriggered = false;

        public QuicksandFreeze(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;

            game.BattleOver += OnBattleOver;
        }

        void OnBattleOver(IGateCard target, ushort winner)
        {
            if (target != this)
                return;
            
            game.OnBattleOver -= this;
            Open();
        }

        public new int TypeId { get; private protected set; } = 6;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
        }

        public new void Open()
        {
            MovingAwayEffectBlocking.Add(this);
            base.Open();
        }

        public void DetermineWinner()
        {
            foreach (Bakugan b in Bakugans)
            {
                b.InBattle = false;
            }
            int[] teamTotals = new int[game.Sides.Length];
            for (int i = 0; i < game.PlayerCount; i++) teamTotals[i] = 0;
            foreach (var b in Bakugans)
            {
                teamTotals[b.Owner.SideID] += b.Power;
            }

            int winnerPower = teamTotals.Max();

            if (teamTotals.Count(x => x == winnerPower) > 1)
            {
                Draw();
                return;
            }

            int winner = Array.IndexOf(teamTotals, teamTotals.Max());

            foreach (Bakugan b in new List<Bakugan>(Bakugans))
                if (b.Owner.SideID != winner)
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

            if (!effectTriggered && IsOpen)
            {
                effectTriggered = true;
                MovingAwayEffectBlocking.Remove(this);
            }
            else (this as IGateCard).Remove();
        }

        public new void Remove()
        {
            base.Remove();
        }

        public bool IsOpenable() =>
            false;
    }
}
