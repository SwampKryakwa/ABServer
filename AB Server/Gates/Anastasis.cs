using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Anastasis : GateCard, IGateCard
    {
        public Anastasis(int cID, Player owner)
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
        }

        public new void Open()
        {
            base.Open();

            game.ContinueGame();
        }

        public void DetermineWinner()
        {
            if (IsOpen)
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
                    b.ToHand(EnterOrder);

                foreach (List<JObject> e in game.NewEvents)
                    e.Add(new JObject
                {
                    { "Type", "BattleOver" },
                    { "IsDraw", false },
                    { "Victor", winner }
                });

                game.OnBattleOver(this, (ushort)winner);

                game.Field[Position.X, Position.Y] = null;

                (this as IGateCard).Remove();
            }
            else
                base.DetermineWinner();
        }

        public new void Remove()
        {
            base.Remove();
        }
    }
}
