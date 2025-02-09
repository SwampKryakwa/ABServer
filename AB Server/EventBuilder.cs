using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System;

namespace AB_Server
{
    internal static class EventBuilder
    {
        public static JObject SelectionBundler(params JObject[] selections)
        {
            return new()
            {
                { "Type", "StartSelection" },
                { "Selections", JArray.FromObject(selections) }
            };
        }

        public static JObject CustomSelectionEvent(string prompt, params string[] options)
        {
            return new()
            {
                { "SelectionType", "C" },
                { "prompt", prompt },
                { "options", JArray.FromObject(options) }
            };
        }

        public static JObject BoolSelectionEvent(string prompt)
        {
            return new()
            {
                { "SelectionType", "Q" },
                { "Prompt", prompt }
            };
        }

        public static JObject CounterSelectionEvent(int userId, int cardId, int cardKind)
        {
            return new()
            {
                { "SelectionType", "R" },
                { "user", userId },
                { "card", cardId },
                { "cardKind", cardKind }
            };
        }

        public static JObject OptionSelectionEvent(string prompt, int options)
        {
            return new JObject
            {
                { "SelectionType", "O" },
                { "Prompt", prompt },
                { "Options", options }
            };
        }

        public static JObject AbilitySelection(string prompt, params AbilityCard[] abilities)
        {
            return new()
            {
                { "SelectionType", "A" },
                { "Message", prompt },
                { "SelectionAbilities", JArray.FromObject(abilities.Select(x => new JObject { { "Type", x.TypeId }, { "Kind", (int)x.Kind }, { "CID", x.CardId } } )) }
            };
        }

        public static JObject HandBakuganSelection(string prompt, int ability, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "BH" },
                { "Message", prompt },
                { "Ability", ability },
                { "SelectionBakugans", new JArray(bakugans.Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.Attribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject FieldBakuganSelection(string prompt, int ability, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "BF" },
                { "Message", prompt },
                { "Ability", ability },
                { "SelectionBakugans", new JArray(bakugans.Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.Attribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject ActiveSelection(string message, params IEnumerable<IActive> actives)
        {
            JArray jsonActives = new();

            foreach (IActive active in actives)
            {
                if (active is AbilityCard activeAbility)
                {
                    jsonActives.Add(new JObject { { "Type", "C" }, { "ActiveOwner", active.Owner.Id }, { "CardType", active.TypeId }, { "CardKind", (int)active.Kind }, { "CID", activeAbility.CardId }, { "EID", active.EffectId } });
                }
                else
                {
                    jsonActives.Add(new JObject { { "Type", "E" }, { "ActiveOwner", active.Owner.Id }, { "CardType", active.TypeId }, { "CardKind", (int)active.Kind }, { "EID", active.EffectId } });
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
            if (RevealInfo)
                return new()
                {
                    { "Type", "GateSetEvent" },
                    { "PosX", card.Position.X },
                    { "PosY", card.Position.Y },
                    { "GateData", new JObject {
                        { "Type", card.TypeId }
                    } },
                    { "Owner", card.Owner.Id },
                    { "CID", card.CardId }
                };
            return new()
            {
                { "Type", "GateSetEvent" },
                { "PosX", card.Position.X },
                { "PosY", card.Position.Y },
                { "GateData", new JObject {
                    { "Type", -1 }
                } },
                { "Owner", card.Owner.Id },
                { "CID", card.CardId }
            };
        }

        public static JObject RemoveGate(GateCard card)
        {
            return new()
            {
                { "Type", "GateRemoved" },
                { "PosX", card.Position.X },
                { "PosY", card.Position.Y }
            };
        }

        public static JObject GateOpen(GateCard card)
        {
            return new()
            {
                { "Type", "GateOpenEvent" },
                { "PosX", card.Position.X },
                { "PosY", card.Position.Y },
                { "GateData", new JObject {
                    { "Type", card.TypeId } }
                },
                { "Owner", card.Owner.Id },
                { "CID", card.CardId }
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

        public static JObject GateNegated(GateCard card)
        {
            return new JObject
            {
                { "Type", "GateNegateEvent" },
                { "PosX", card.Position.X },
                { "PosY", card.Position.Y },
                { "Owner", card.Owner.Id },
                { "CID", card.CardId }
            };
        }

        public static JObject GateRetracted(GateCard card)
        {
            return new JObject
            {
                { "Type", "GateRetracted" },
                { "PosX", card.Position.X },
                { "PosY", card.Position.Y },
            };
        }

        public static JObject AbilityActivated(AbilityCard card)
        {
            return new JObject
            {
                { "Type", "AbilityActivated" },
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
