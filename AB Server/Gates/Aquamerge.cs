namespace AB_Server.Gates
{
    internal class Aquamerge : GateCard
    {
        public Aquamerge(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 7;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Push(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }
        public override void Resolve()
        {
            foreach (var bakugan in Bakugans.Where(x => !x.IsAttribute(Attribute.Subterra)))
            {
                bakugan.ChangeAttribute(Attribute.Aqua, this);
                bakugan.AffectingEffects.Add(this);
            }

            game.BakuganReturned += OnBakuganReturned;

            game.ChainStep();
        }

        private void OnBakuganReturned(Bakugan target, byte owner)
        {
            if (target.AffectingEffects.Contains(this))
                target.ChangeAttribute(target.BaseAttribute, this);
        }
    }
}
