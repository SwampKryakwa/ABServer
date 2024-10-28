using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Numerics;

namespace AB_Server.Abilities
{
    internal class TidalInsight : AbilityCard, IAbilityCard
    {
        public TidalInsight(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var ctrs = AbilityCtrs.Where(x => x.validTarget(User)).ToList();
            var ability = ctrs[new Random().Next(ctrs.Count)].constructor(Game.AbilityIndex.Count, Owner);
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
            user.OnField() && user.Attribute == Attribute.Aqua;
    }
}
