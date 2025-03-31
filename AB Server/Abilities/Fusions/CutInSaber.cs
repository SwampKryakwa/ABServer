using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AB_Server.Abilities
{
    internal class CutInSaber : FusionAbility
    {
        public CutInSaber(int cID, Player owner)
        {
            TypeId = 3;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(CutInSlayer);
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.HandBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = PickTarget;
        }

        public void PickTarget()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldGateSelection("INFO_SELECT_GATE", TypeId, (int)Kind, Game.GateIndex.Where(g => g.Bakugans.Count >= 2 && g.Freezing.Count == 0))
            ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        GateCard targetGate;
        public new void Activate()
        {
            targetGate = Game.GateIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["gate"]];

            FusedTo.Discard();

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    ["Type"] = "AbilityAddedActiveZone",
                    ["IsCopy"] = IsCopy,
                    ["Id"] = EffectId,
                    ["Card"] = TypeId,
                    ["Kind"] = (int)Kind,
                    ["User"] = User.BID,
                    ["IsCounter"] = asCounter,
                    ["Owner"] = Owner.Id
                });
            }

            Game.CheckChain(Owner, this, User);

            if (User.InHands)
                User.AddFromHand(targetGate);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new CutInSaberEffect(User, targetGate, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new CutInSaberEffect(User, targetGate, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleStart && user.Type == BakuganType.Tigress && user.IsPartner && user.InHands;
    }

    internal class CutInSaberEffect
    {
        public int TypeId { get; }
        Bakugan user;
        GateCard targetGate;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public CutInSaberEffect(Bakugan user, GateCard targetGate, int typeID, bool IsCopy)
        {
            this.user = user;
            this.targetGate = targetGate;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "FusionAbilityActivateEffect" },
                    { "Kind", 1 },
                    { "Card", TypeId },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Treatment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            if (user.InHands)
                user.AddFromHand(targetGate);
        }
    }
}
