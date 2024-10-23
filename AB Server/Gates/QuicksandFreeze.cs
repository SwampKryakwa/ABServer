﻿using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class QuicksandFreeze : GateCard, IGateCard
    {
        public QuicksandFreeze(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            DisallowedPlayers = new bool[game.PlayerCount];
            for (int i = 0; i < game.PlayerCount; i++)
                DisallowedPlayers[i] = false;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 6;

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
                for (int i = 0; i < DisallowedPlayers.Length; i++)
                {
                    DisallowedPlayers[i] = false;
                }
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
