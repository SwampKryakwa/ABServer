﻿using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class HolyLight : AbilityCard
    {
        public HolyLight(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BG", ForPlayer = owner.Id, Message = "INFO_ABILITY_REVIVETARGET", TargetValidator = x => x.Owner == Owner && x.Power == Game.BakuganIndex.Where(x=>x.InGrave()).MinBy(x=>x.Power).Power && x.InGrave()}
            ];
        }

        public override void TriggerEffect() =>
                new HolyLightEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Lumina) && user.OnField() && Owner.BakuganGrave.Bakugans.Count != 0;

        public static new bool HasValidTargets(Bakugan user) =>
            user.Owner.BakuganGrave.Bakugans.Count != 0;
    }

    internal class HolyLightEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }


        public Player Onwer { get; set; }
        bool IsCopy;

        public HolyLightEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;

            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
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
            }
            target.Revive();
        }
    }
}
