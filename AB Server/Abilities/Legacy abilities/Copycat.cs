using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Numerics;

namespace AB_Server.Abilities
{
    internal class Copycat : AbilityCard
    {
        public Copycat(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Setup(bool asCounter)
        {
            AbilityCard ability = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_BOOSTTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_ABILITYNEGATETARGET", Game.ActiveZone.Where(x => x.ActiveType == ActiveType.Card).ToArray())
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public void SetupFusion(AbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            if (parentCard != null) parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_ABILITYNEGATETARGET", Game.ActiveZone.Where(x => x.ActiveType == ActiveType.Card).ToArray())
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        AbilityCard target;

        public void Activate()
        {
            target = (AbilityCard)Game.ActiveZone.First(x => x.EffectId == (int)Game.IncomingSelection[Owner.Id]["array"][0]["active"]);

            if (!AbilityCtrs[target.TypeId].validTarget(User))
                Game.CheckChain(Owner, this, User);

            var ability = AbilityCtrs[target.TypeId].constructor(Game.AbilityIndex.Count, Owner);
            ability.IsCopy = true;
            Game.AbilityIndex.Add(ability);

            Game.AbilityChain.Insert(Game.AbilityChain.IndexOf(this) + 1, ability);
            ability.EffectId = Game.NextEffectId++;
            Game.ActiveZone.Add(ability);
            Owner.AbilityHand.Remove(ability);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    ["Type"] = "AbilityAddedActiveZone",
                    ["IsCopy"] = ability.IsCopy,
                    ["Id"] = ability.EffectId,
                    ["Card"] = ability.TypeId,
                    ["Owner"] = ability.Owner.Id
                });
            }

            ability.SetupFusion(null, User);
        }

        public new void Resolve() =>
            Dispose();

        public new void DoubleEffect()
        {
            return;
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Subterra && Game.ActiveZone.Any(x => x.ActiveType == ActiveType.Card);
    }
}
