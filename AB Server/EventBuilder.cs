using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

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

        public static JObject CustomSelectionEvent(string prompt, params IEnumerable<string> options)
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

        public static JObject AbilitySelection(string prompt, params IEnumerable<AbilityCard> abilities)
        {
            return new()
            {
                { "SelectionType", "A" },
                { "Message", prompt },
                { "SelectionAbilities", JArray.FromObject(abilities.Select(x => new JObject { { "Type", x.TypeId }, { "Kind", (int)x.Kind }, { "CID", x.CardId } } )) }
            };
        }

        public static JObject AnyBakuganSelection(string prompt, int ability, int abilityKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "B" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionFieldBakugans", new JArray(bakugans.Where(x=>x.OnField()).Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) },
                { "SelectionHandBakugans", new JArray(bakugans.Where(x=>x.InHand()).Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject HandBakuganSelection(string prompt, int ability, int abilityKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "BH" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionBakugans", new JArray(bakugans.Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject FieldBakuganSelection(string prompt, int ability, int abilityKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "BF" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionBakugans", new JArray(bakugans.Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject GraveBakuganSelection(string prompt, int ability, int abilityKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "BG" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionBakugans", new JArray(bakugans.Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject AnyMultiBakuganSelection(string prompt, int ability, int abilityKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "MB" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionFieldBakugans", new JArray(bakugans.Where(x=>x.OnField()).Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) },
                { "SelectionHandBakugans", new JArray(bakugans.Where(x=>x.InHand()).Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject HandMultiBakuganSelection(string prompt, int ability, int abilityKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "MBH" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionBakugans", new JArray(bakugans.Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject FieldMultiBakuganSelection(string prompt, int ability, int abilityKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "MBF" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionBakugans", new JArray(bakugans.Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject GraveMultiBakuganSelection(string prompt, int ability, int abilityKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                { "SelectionType", "MBG" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionBakugans", new JArray(bakugans.Select(x => new JObject {
                    { "Type", (int)x.Type },
                    { "Attribute", (int)x.MainAttribute },
                    { "Treatment", (int)x.Treatment },
                    { "Power", x.Power },
                    { "Owner", x.Owner.Id },
                    { "IsPartner", x.IsPartner },
                    { "BID", x.BID }
                }) ) }
            };
        }

        public static JObject FieldGateSelection(string prompt, int ability, int abilityKind, params IEnumerable<GateCard> gates)
        {
            return new()
            {
                { "SelectionType", "GF" },
                { "Message", prompt },
                { "Card", ability },
                { "CardKind", abilityKind },
                { "SelectionGates", new JArray(gates.Select(x => new JObject {
                    { "Type", x.TypeId },
                    { "PosX", x.Position.X },
                    { "PosY", x.Position.Y },
                    { "CID", x.CardId }
                }) ) }
            };
        }

        public static JObject ActiveSelection(string message, int kind, params IEnumerable<IActive> actives)
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
                    jsonActives.Add(new JObject { { "Type", "E" }, {"ActiveOwner", active.Owner.Id }, { "CardType", active.TypeId }, { "CardKind", (int)active.Kind }, { "EID", active.EffectId } });
                }
            }

            return new JObject
            {
                { "SelectionType", "AC" },
                { "Message", message },
                { "CardKind", kind },
                { "SelectionAbilities", jsonActives }
            };
        }

        public static JObject GateSet(GateCard card, bool RevealInfo)
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

        public static JObject SendGateToGrave(GateCard card) =>
            new()
            {
                ["Type"] = "GateSentToGrave",
                ["CardType"] = card.TypeId,
                ["CID"] = card.CardId,
                ["Owner"] = card.Owner.Id
            };

        public static JObject SendAbilityToGrave(AbilityCard card) =>
            new()
            {
                ["Type"] = "AbilitySentToGrave",
                ["Kind"] = (int)card.Kind,
                ["CardType"] = card.TypeId,
                ["CID"] = card.CardId,
                ["Owner"] = card.Owner.Id
            };

        public static JObject AddEffectToActiveZone(IActive active, bool isCopy) =>
            new() {
                { "Type", "EffectAddedActiveZone" },
                { "IsCopy", isCopy },
                { "Card", active.TypeId },
                { "Kind", (int)active.Kind },
                { "Id", active.EffectId },
                { "User", active.User.BID },
                { "Owner", active.Owner.Id }
            };

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

        public static JObject ActivateAbilityEffect(int abilityType, int abilityKind, Bakugan user) =>
            new() {
                { "Type", "AbilityActivateEffect" },
                { "Kind", abilityKind },
                { "Card", abilityType },
                { "UserID", user.BID },
                { "User", new JObject {
                    { "Type", (int)user.Type },
                    { "Attribute", (int)user.MainAttribute },
                    { "Treatment", (int)user.Treatment },
                    { "Power", user.Power }
                }}
            };
    }
}
