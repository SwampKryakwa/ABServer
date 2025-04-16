using System.Linq;

namespace AB_Server.Gates
{
    internal class Intercept : GateCard
    {
        private int turnCounter = 0;

        public Intercept(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            CardId = cID;

        }

        public override int TypeId { get; } = 11;

        public override bool CheckBattles()
        {
            if (Freezing.Count != 0 || BattleOver) return false;

            bool isBattle = Bakugans.Count > 1;

            if (isBattle)
            {
                if (!ActiveBattle)
                {
                    game.BattlesToStart.Add(this);
                }
            }
            else
            {
                ActiveBattle = false;
            }

            return isBattle;
        }

        public override void StartBattle()
        {
            if (!IsOpen && !Negated)
                Open();
            else
                base.StartBattle();
        }

        private void OnTurnAboutToEnd()
        {
            if (turnCounter++ > 1)
            {
                TryUnfreeze(this);
                game.TurnAboutToEnd -= OnTurnAboutToEnd;
            }
        }

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            ThrowBlocking.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));
            game.CheckChain(Owner, this);
        }

        public override void Negate(bool asCounter = false)
        {
            base.Negate(asCounter);

            ThrowBlocking.Remove(this);
        }

        public override void Resolve()
        {
            Freeze(this);

            game.TurnAboutToEnd += OnTurnAboutToEnd;
        }

        public override bool IsOpenable() => false;
    }
}
