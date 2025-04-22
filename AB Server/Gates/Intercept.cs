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
            ThrowBlocking.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));
            game.NextStep();
        }

        public override void Negate(bool asCounter = false)
        {
            base.Negate(asCounter);

            ThrowBlocking.Remove(this);
        }

        public override void Resolve()
        {
            game.ActiveZone.Remove(this);
            Freeze(this);

            game.TurnAboutToEnd += OnTurnAboutToEnd;
        }

        public override bool IsOpenable() => false;
    }
}
