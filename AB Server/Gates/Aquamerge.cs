namespace AB_Server.Gates
{
    internal class Aquamerge : GateCard
    {
        public Aquamerge(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 7;

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
            foreach (var bakugan in Bakugans.Where(x => x.Attribute != Attribute.Aqua))
            {
                bakugan.ChangeAttribute(Attribute.Aqua, this);
                bakugan.affectingEffects.Add(this);
            }

            game.BakuganReturned += OnBakuganReturned;

            game.ContinueGame();
        }

        private void OnBakuganReturned(Bakugan target, byte owner)
        {
            if (target.affectingEffects.Contains(this))
                target.ChangeAttribute(target.BaseAttribute, this);
        }
    }
}
