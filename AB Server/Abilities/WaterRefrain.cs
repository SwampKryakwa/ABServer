﻿using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class WaterRefrain : AbilityCard
    {
        public WaterRefrain(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        { }

        public override void TriggerEffect() =>
                new WaterRefrainEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Owner.BakuganOwned.All(x=>x.IsAttribute(Attribute.Aqua)) && Game.CurrentWindow == ActivationWindow.TurnStart && Game.TurnPlayer != Owner.Id && user.IsAttribute(Attribute.Aqua) && user.OnField();
    }

    internal class WaterRefrainEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public Bakugan User { get; set; }
        Game game { get => User.Game; }
        int turnsPassed = 0;

        public Player Owner { get; set; }
        public CardKind Kind { get; } = CardKind.NormalAbility;
        bool IsCopy;

        public WaterRefrainEffect(Bakugan user, int typeID, bool IsCopy)
        {
            User = user;
             this.IsCopy = IsCopy; Owner = user.Owner;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
            int team = User.Owner.SideID;
            game.ActiveZone.Add(this);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.MainAttribute },
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
                game.NewEvents[i].Add(EventBuilder.AddEffectToActiveZone(this, IsCopy));
            }
            game.Players.Where(x=>x.SideID != Owner.SideID).ToList().ForEach(p => p.AbilityBlockers.Add(this));

            game.TurnEnd += CheckEffectOver;

            User.affectingEffects.Add(this);
        }

        //is not negatable after turn ends
        public void CheckEffectOver()
        {
            if (turnsPassed == 1)
            {
                game.ActiveZone.Remove(this);
                game.Players.ForEach(x => { if (x.AbilityBlockers.Contains(this)) x.AbilityBlockers.Remove(this); });
                game.TurnEnd -= CheckEffectOver;

                for (int i = 0; i < game.NewEvents.Length; i++)
                {
                    game.NewEvents[i].Add(new() {
                        { "Type", "EffectRemovedActiveZone" },
                        { "Id", EffectId }
                    });
                }
            }

            if (game.TurnPlayer != Owner.Id) turnsPassed++;
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);
            game.Players.ForEach(x => { if (x.AbilityBlockers.Contains(this)) x.AbilityBlockers.Remove(this); });
            game.TurnEnd -= CheckEffectOver;

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectRemovedActiveZone" },
                    { "Id", EffectId }
                });
            }
        }
    }
}
