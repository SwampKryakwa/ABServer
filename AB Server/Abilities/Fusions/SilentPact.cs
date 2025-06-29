namespace AB_Server.Abilities.Fusions
{
    internal class SilentPact : FusionAbility
    {
        public SilentPact(int cID, Player owner) : base(cID, owner, 13, typeof(SlashZero))
        {
            CondTargetSelectors =
            [
                new OptionSelector() { Message = "INFO_PICKER_ATTRIBUTE", ForPlayer = (p) => p == Owner, OptionCount = 6 }
            ];
        }

        public override void TriggerEffect()
        {
            new SilentPactMarker(User, (Attribute)(CondTargetSelectors[0] as OptionSelector)!.SelectedOption, Owner, IsCopy).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Shredder && user.OnField();
    }

    internal class SilentPactMarker(Bakugan user, Attribute newAttribute, Player owner, bool isCopy) : IActive
    {
        public int EffectId { get; set; } = owner.Game.NextEffectId++;

        public int TypeId { get => 13; }

        public CardKind Kind { get => CardKind.FusionAbility; }

        public Bakugan User { get; set; } = user;
        public Player Owner { get; set; } = owner;

        Attribute oldAttribute;

        public void Activate()
        {
            User.Game.ActiveZone.Add(this);

            User.Game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));

            oldAttribute = User.ChangeAttribute(newAttribute, this);
            User.Game.BakuganReturned += OnBakuganRemovedFromField;
            User.Game.BakuganDestroyed += OnBakuganRemovedFromField;
        }

        private void OnBakuganRemovedFromField(Bakugan target, byte owner)
        {
            User.Game.ActiveZone.Remove(this);

            User.Game.BakuganReturned -= OnBakuganRemovedFromField;
            User.Game.BakuganDestroyed -= OnBakuganRemovedFromField;

            User.ChangeAttribute(oldAttribute, this);

            User.Game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        }

        public void Negate(bool asCounter = false)
        {
            User.Game.ActiveZone.Remove(this);

            User.Game.BakuganReturned -= OnBakuganRemovedFromField;
            User.Game.BakuganDestroyed -= OnBakuganRemovedFromField;

            User.ChangeAttribute(oldAttribute, this);

            User.Game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        }
    }
}
