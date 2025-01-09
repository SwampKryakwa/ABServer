using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server
{
    internal static class EventBuilder
    {
        public static JObject SelectionBundler(params JObject[] selections)
        {
            return new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", JArray.FromObject(selections) }
            };
        }

        public static JObject CustomSelectionEvent(string prompt, params string[] options)
        {
            return new JObject
            {
                { "SelectionType", "C" },
                { "prompt", prompt },
                { "options", JArray.FromObject(options) }
            };
        }

        public static JObject BoolSelectionEvent(string prompt)
        {
            return new JObject
            {
                { "SelectionType", "Q" },
                { "Prompt", prompt }
            };
        }

        public static JObject CounterSelectionEvent(int userId, int cardId, char counterableType)
        {
            return new JObject
            {
                { "SelectionType", "R" },
                { "user", userId },
                { "card", cardId },
                { "counterableType", counterableType }
            };
        }

        public static JObject AbilitySelection(string prompt, params AbilityCard[] abilities)
        {
            return new JObject
            {
                { "SelectionType", "A" },
                { "Message", prompt },
                { "SelectionAbilities", JArray.FromObject(abilities.Select(x => new JObject { { "Type", x.TypeId }, { "CID", x.CardId } } )) }
            };
        }

        public static JObject ActiveSelection(string message, params IActive[] actives)
        {
            JArray jsonActives = new();

            foreach (IActive active in actives)
            {
                if (active is AbilityCard activeAbility)
                {
                    jsonActives.Add(new JObject { { "Type", "C" }, { "ActiveOwner", active.Owner.Id }, { "CardType", active.TypeId }, { "CID", activeAbility.CardId }, { "EID", active.EffectId } });
                }
                else
                {
                    jsonActives.Add(new JObject { { "Type", "E" }, { "ActiveOwner", active.Owner.Id }, { "CardType", active.TypeId }, { "EID", active.EffectId } });
                }
            }

            return new JObject
            {
                { "SelectionType", "AC" },
                { "Message", message },
                { "SelectionAbilities", jsonActives }
            };
        }

        public static JObject SetGate(GateCard card, bool RevealInfo)
        {
            JObject extra = new();

            //switch (card.TypeId)
            //{
            //    //case 0:
            //    //    extra = new JObject
            //    //    {
            //    //        { "Attribute", (int)(card as NormalGate).Attribute },
            //    //        { "Power", (card as NormalGate).Power }
            //    //    };
            //    //    break;
            //    //case 4:
            //    //    extra = new JObject
            //    //    {
            //    //        { "Attribute", (int)(card as AttributeHazard).Attribute },
            //    //    };
            //    //    break;
            //}

            if (RevealInfo)
                return new JObject
                {
                    { "Type", "SetGate" },
                    { "type", card.TypeId },
                    { "owner", card.Owner.Id },
                    { "posX", card.Position.X },
                    { "posY", card.Position.Y },
                    { "extra", extra }
                };
            return new JObject
            {
                { "Type", "SetGate" },
                { "type", -1 },
                { "owner", card.Owner.Id },
                { "posX", card.Position.X },
                { "posY", card.Position.Y }
            };
        }

        public static JObject RemoveGate(GateCard card)
        {
            return new JObject
            {
                { "Type", "RemoveGate" },
                { "posX", card.Position.X },
                { "posY", card.Position.Y }
            };
        }

        public static JObject OpenGate(GateCard card)
        {
            JObject extra = new();

            //switch (card.TypeId)
            //{
            //    //case 0:
            //    //    extra = new JObject
            //    //    {
            //    //        { "Attribute", (int)(card as NormalGate).Attribute },
            //    //        { "Power", (card as NormalGate).Power }
            //    //    };
            //    //    break;
            //    //case 4:
            //    //    extra = new JObject
            //    //    {
            //    //        { "Attribute", (int)(card as AttributeHazard).Attribute },
            //    //    };
            //    //    break;
            //}

            return new JObject
            {
                { "Type", "OpenGate" },
                { "type", card.TypeId },
                { "owner", card.Owner.Id },
                { "posX", card.Position.X },
                { "posY", card.Position.Y },
                { "extra", extra }
            };
        }

        public static JObject DiscardGate(GateCard card)
        {
            JObject extra = new();

            //switch (card.TypeId)
            //{
            //    //case 0:
            //    //    extra = new JObject
            //    //    {
            //    //        { "Attribute", (int)(card as NormalGate).Attribute },
            //    //        { "Power", (card as NormalGate).Power }
            //    //    };
            //    //    break;
            //    //case 4:
            //    //    extra = new JObject
            //    //    {
            //    //        { "Attribute", (int)(card as AttributeHazard).Attribute },
            //    //    };
            //    //    break;
            //}

            return new JObject
            {
                { "Type", "DiscardGate" },
                { "type", card.TypeId },
                { "owner", card.Owner.Id },
                { "extra", extra }
            };
        }

        public static JObject RetractGate(GateCard card)
        {
            return new JObject
            {
                { "Type", "OpenGate" },
                { "posX", card.Position.X },
                { "posY", card.Position.Y },
            };
        }

        public static JObject ActivateAbility(AbilityCard card)
        {
            return new JObject
            {
                { "Type", "ActivateAbility" },
                { "type", card.TypeId },
                { "owner", card.Owner.Id },
            };
        }

        public static JObject NegateAbility(AbilityCard card)
        {
            return new JObject
            {
                { "Type", "NegateAbility" },
                { "type", card.TypeId },
                { "owner", card.Owner.Id },
            };
        }

        public static JObject DiscardAbility(AbilityCard card)
        {
            return new JObject
            {
                { "Type", "DiscardAbility" },
                { "type", card.TypeId },
                { "owner", card.Owner.Id },
            };
        }

        public static JObject RestoreAbility(AbilityCard card)
        {
            return new JObject
            {
                { "Type", "RestoreAbility" },
                { "type", card.TypeId },
                { "owner", card.Owner.Id },
            };
        }

        public static JObject ThrowBakugan(Bakugan bakugan, int posX, int posY)
        {
            return new JObject
            {
                { "Type", "ThrowBakugan" },
                { "type", (int)bakugan.Type },
                { "attribute", (int)bakugan.Attribute },
                { "treatment", (int)bakugan.Treatment },
                { "power", bakugan.Power },
                { "id", bakugan.BID },
                { "owner", bakugan.Owner.Id },
                { "posX", posX },
                { "posY", posY },
            };
        }

        public static JObject AddBakugan(Bakugan bakugan, int posX, int posY)
        {
            return new JObject
            {
                { "Type", "AddBakugan" },
                { "type", (int)bakugan.Type },
                { "attribute", (int)bakugan.Attribute },
                { "treatment", (int)bakugan.Treatment },
                { "power", bakugan.Power },
                { "id", bakugan.BID },
                { "owner", bakugan.Owner.Id },
                { "posX", posX },
                { "posY", posY },
            };
        }

        public static JObject DestroyBakugan(Bakugan bakugan)
        {
            return new JObject
            {
                { "Type", "DestroyBakugan" },
                { "type", (int)bakugan.Type },
                { "attribute", (int)bakugan.Attribute },
                { "treatment", (int)bakugan.Treatment },
                { "power", bakugan.Power },
                { "owner", bakugan.Owner.Id }
            };
        }

        public static JObject RemoveBakugan(Bakugan bakugan, int posX, int posY, bool silent)
        {
            return new JObject
            {
                { "Type", "RemoveBakugan" },
                { "id", bakugan.BID },
                { "posX", posX },
                { "posY", posY },
                { "silent", silent }
            };
        }

        public static JObject RetractBakugan(Bakugan bakugan, int posX, int posY)
        {
            return new JObject
            {
                { "Type", "RetractBakugan" },
                { "type", (int)bakugan.Type },
                { "attribute", (int)bakugan.Attribute },
                { "treatment", (int)bakugan.Treatment },
                { "power", bakugan.Power },
                { "owner", bakugan.Owner.Id },
                { "posX", posX },
                { "posY", posY },
            };
        }

        public static JObject BoostBakugan(Bakugan bakugan)
        {
            return new JObject
            {
                { "Type", "BoostBakugan" },
                { "type", (int)bakugan.Type },
                { "attribute", (int)bakugan.Attribute },
                { "treatment", (int)bakugan.Treatment },
                { "power", bakugan.Power },
                { "owner", bakugan.Owner.Id }
            };
        }

        public static JObject BoostBakuganHand(Bakugan bakugan)
        {
            return new JObject
            {
                { "Type", "BoostBakuganHand" },
                { "type", (int)bakugan.Type },
                { "attribute", (int)bakugan.Attribute },
                { "treatment", (int)bakugan.Treatment },
                { "power", bakugan.Power },
                { "owner", bakugan.Owner.Id }
            };
        }

        public static JObject BaseBoostBakugan(Bakugan bakugan)
        {
            return new JObject
            {
                { "Type", "BaseBoostBakugan" },
                { "type", (int)bakugan.Type },
                { "attribute", (int)bakugan.Attribute },
                { "treatment", (int)bakugan.Treatment },
                { "power", bakugan.Power },
                { "owner", bakugan.Owner.Id }
            };
        }

        public static JObject BaseBoostBakuganHand(Bakugan bakugan)
        {
            return new JObject
            {
                { "Type", "BaseBoostBakuganHand" },
                { "type", (int)bakugan.Type },
                { "attribute", (int)bakugan.Attribute },
                { "treatment", (int)bakugan.Treatment },
                { "power", bakugan.Power },
                { "owner", bakugan.Owner.Id }
            };
        }

        public static JObject SpecialThrowUsed(int type, int user)
        {
            return new JObject
            {
                { "Type", "SpecialThrowUsed" },
                { "type", type },
                { "user", user }
            };
        }

        public static JObject ContEffectStart(int type)
        {
            return new JObject
            {
                { "Type", "ContEffectStart" },
                { "type", type }
            };
        }

        public static JObject ContEffectStop(int type)
        {
            return new JObject
            {
                { "Type", "ContEffectStop" },
                { "type", type }
            };
        }

        public static JObject GateBattleStart(int posX, int posY)
        {
            return new JObject
            {
                { "Type", "SpecialThrowUsed" },
                { "posX", posX },
                { "posY", posY }
            };
        }
    }
}
