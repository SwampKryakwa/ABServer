﻿using System.Runtime.InteropServices;

namespace AB_Server.Abilities
{
    internal class WaterRefrain : AbilityCard
    {
        public WaterRefrain(int cID, Player owner, int typeId) : base(cID, owner, typeId) { }

        public override void TriggerEffect() =>
                new WaterRefrainMarker(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Aqua)) && Game.CurrentWindow == ActivationWindow.TurnStart && Game.TurnPlayer != Owner.Id && user.IsAttribute(Attribute.Aqua) && user.OnField();
    }

    internal class WaterRefrainMarker : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public Bakugan User { get; set; }
        Game game { get => User.Game; }
        int turnsPassed = 0;

        public Player Owner { get; set; }
        public CardKind Kind { get; } = CardKind.NormalAbility;
        bool IsCopy;

        public WaterRefrainMarker(Bakugan user, int typeID, bool IsCopy)
        {
            User = user;
            this.IsCopy = IsCopy; Owner = user.Owner;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
            int team = User.Owner.TeamId;
            game.ActiveZone.Add(this);

            
            game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));
            game.Players.Where(x => x.TeamId != Owner.TeamId).ToList().ForEach(p => p.AbilityBlockers.Add(this));

            game.TurnEnd += CheckEffectOver;

            User.AffectingEffects.Add(this);
        }

        //is not negatable after turn ends
        public void CheckEffectOver()
        {
            if (turnsPassed++ == 1)
                StopEffect();
        }

        public void Negate(bool asCounter) => StopEffect();

        void StopEffect()
        {
            game.ActiveZone.Remove(this);
            Array.ForEach(game.Players, x => { if (x.AbilityBlockers.Contains(this)) x.AbilityBlockers.Remove(this); });
            game.TurnEnd -= CheckEffectOver;

            game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        }
    }
}
