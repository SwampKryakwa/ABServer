namespace AB_Server.Gates
{
    internal class PositiveDelta : GateCard
    {
        public PositiveDelta(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 6;

        public override bool CheckBattles()
        {
            if (IsFrozen || BattleOver) return false;

            bool isBattle = Bakugans.Count > 1;

            if (isBattle)
            {
                if (!ActiveBattle)
                {
                    game.BattlesToStart.Add(this);
                    if (!IsOpen && !Negated)
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
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));
            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (Bakugans.Any(x => x.MainAttribute == Attribute.Nova || x.MainAttribute == Attribute.Aqua || x.MainAttribute == Attribute.Lumina))
                foreach (var bakugan in Bakugans.Where(x => x.MainAttribute == Attribute.Nova || x.MainAttribute == Attribute.Aqua || x.MainAttribute == Attribute.Lumina))
                    bakugan.Boost(new Boost(-200), this);
            else
                foreach (var bakugan in Bakugans)
                    bakugan.Boost(new Boost(-200), this);
        }

        public override bool IsOpenable() =>
            false;
    }
}
