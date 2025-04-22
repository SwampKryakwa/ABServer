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

        public override void Open()
        {
            IsOpen = true;
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));
            game.NextStep();
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
