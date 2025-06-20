﻿namespace AB_Server.Gates
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
            if (OpenBlocking.Count == 0 && !IsOpen && !Negated)
                game.AutoGatesToOpen.Add(this);
        }

        public override void Open()
        {
            IsOpen = true;
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (Bakugans.Any(x => x.IsAttribute(Attribute.Darkon)))
                BattleDeclaredOver = true;

            game.ChainStep();
        }
    }
}
