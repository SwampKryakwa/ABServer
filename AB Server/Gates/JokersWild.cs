﻿namespace AB_Server.Gates
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
            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            DetermineWinner();
        }

        public override bool IsOpenable() =>
            false;
    }
}
