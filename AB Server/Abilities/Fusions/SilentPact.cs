﻿namespace AB_Server.Abilities.Fusions
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

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]];

            Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.HandBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
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

        AttributeState attributeState;

        public void Activate()
        {
            User.Game.ActiveZone.Add(this);

            User.Game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));

            attributeState = User.ChangeAttribute(newAttribute, this);
            User.OnRemovedFromField += Stop;
            User.OnFromHandToDrop += Refresh;
            User.OnFromDropToHand += Refresh;

        }

        private void Stop()
        {
            User.Game.ActiveZone.Remove(this);

            User.OnRemovedFromField -= Stop;
            User.OnFromHandToDrop -= Refresh;
            User.OnFromDropToHand -= Refresh;

            User.RevertAttributeChange(attributeState, this);

            User.Game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        }

        private void Refresh()
        {
            User.ChangeAttribute(newAttribute, this);
        }

        public void Negate(bool asCounter = false) => Stop();
    }
}
