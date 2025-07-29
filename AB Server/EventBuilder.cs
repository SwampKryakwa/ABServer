using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server
{
    internal static class EventBuilder
    {
        public static JObject SelectionBundler(bool canCancel, params JObject[] selections)
        {
            return new()
            {
                ["Type"] = "StartSelection",
                ["CanCancel"] = canCancel,
                ["Selections"] = JArray.FromObject(selections)
            };
        }

        public static JObject CustomSelectionEvent(string prompt, params IEnumerable<string> options)
        {
            return new()
            {
                ["SelectionType"] = "C",
                ["prompt"] = prompt,
                ["options"] = JArray.FromObject(options)
            };
        }

        public static JObject BoolSelectionEvent(string prompt)
        {
            return new()
            {
                ["SelectionType"] = "Q",
                ["Prompt"] = prompt
            };
        }

        public static JObject CounterSelectionEvent(int userId, int cardId, int cardKind)
        {
            return new()
            {
                ["SelectionType"] = "R",
                ["user"] = userId,
                ["card"] = cardId,
                ["cardKind"] = cardKind
            };
        }

        public static JObject OptionSelectionEvent(string prompt, int options)
        {
            return new JObject
            {
                ["SelectionType"] = "O",
                ["Prompt"] = prompt,
                ["Options"] = options
            };
        }

        public static JObject AbilitySelection(string prompt, params IEnumerable<AbilityCard> abilities)
        {
            return new()
            {
                ["SelectionType"] = "A",
                ["Message"] = prompt,
                ["SelectionAbilities"] = JArray.FromObject(abilities.Select(x => new JObject
                {
                    ["Type"] = x.TypeId,
                    ["Kind"] = (int)x.Kind,
                    ["CID"] = x.CardId
                }))
            };
        }

        public static JObject AnyBakuganSelection(string prompt, int card, int cardKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                ["SelectionType"] = "B",
                ["Message"] = prompt,
                ["Card"] = card,
                ["CardKind"] = cardKind,
                ["SelectionFieldBakugans"] = new JArray(bakugans.Where(x => x.OnField()).Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
                ["SelectionHandBakugans"] = new JArray(bakugans.Where(x => x.InHand()).Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
                ["SelectionGraveBakugans"] = new JArray(bakugans.Where(x => x.InDrop()).Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                }))
            };
        }

        public static JObject HandBakuganSelection(string prompt, int card, int cardKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                ["SelectionType"] = "BH",
                ["Message"] = prompt,
                ["Card"] = card,
                ["CardKind"] = cardKind,
                ["SelectionBakugans"] = new JArray(bakugans.Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
            };
        }

        public static JObject FieldBakuganSelection(string prompt, int card, int cardKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                ["SelectionType"] = "BF",
                ["Message"] = prompt,
                ["Card"] = card,
                ["CardKind"] = cardKind,
                ["SelectionBakugans"] = new JArray(bakugans.Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
            };
        }

        public static JObject DropBakuganSelection(string prompt, int card, int cardKind, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                ["SelectionType"] = "BG",
                ["Message"] = prompt,
                ["Card"] = card,
                ["CardKind"] = cardKind,
                ["SelectionBakugans"] = new JArray(bakugans.Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
            };
        }

        public static JObject AnyMultiBakuganSelection(string prompt, int card, int cardKind, int min, int max, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                ["SelectionType"] = "MB",
                ["Message"] = prompt,
                ["Card"] = card,
                ["CardKind"] = cardKind,
                ["SelectionFieldBakugans"] = new JArray(bakugans.Where(x => x.OnField()).Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
                ["SelectionHandBakugans"] = new JArray(bakugans.Where(x => x.InHand()).Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
                ["SelectionGraveBakugans"] = new JArray(bakugans.Where(x => x.InDrop()).Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
                ["Min"] = min,
                ["Max"] = max
            };
        }

        public static JObject HandMultiBakuganSelection(string prompt, int card, int cardKind, int min, int max, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                ["SelectionType"] = "MBH",
                ["Message"] = prompt,
                ["Card"] = card,
                ["CardKind"] = cardKind,
                ["SelectionBakugans"] = new JArray(bakugans.Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
                ["Min"] = min,
                ["Max"] = max
            };
        }

        public static JObject FieldMultiBakuganSelection(string prompt, int card, int cardKind, int min, int max, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                ["SelectionType"] = "MBF",
                ["Message"] = prompt,
                ["Card"] = card,
                ["CardKind"] = cardKind,
                ["SelectionBakugans"] = new JArray(bakugans.Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
                ["Min"] = min,
                ["Max"] = max
            };
        }

        public static JObject DropMultiBakuganSelection(string prompt, int card, int cardKind, int min, int max, params IEnumerable<Bakugan> bakugans)
        {
            return new()
            {
                ["SelectionType"] = "MBG",
                ["Message"] = prompt,
                ["Card"] = card,
                ["CardKind"] = cardKind,
                ["SelectionBakugans"] = new JArray(bakugans.Select(x => new JObject
                {
                    ["Type"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["Power"] = x.Power,
                    ["Owner"] = x.Owner.Id,
                    ["IsPartner"] = x.IsPartner,
                    ["BID"] = x.BID
                })),
                ["Min"] = min,
                ["Max"] = max
            };
        }

        public static JObject FieldSlotSelection(string prompt, int ability, int cardKind)
        {
            return new()
            {
                ["SelectionType"] = "S",
                ["Message"] = prompt,
                ["Card"] = ability,
                ["CardKind"] = cardKind
            };
        }

        public static JObject MultiFieldSlotSelection(string prompt, int ability, int cardKind, int min = 0, int max = 7)
        {
            return new()
            {
                ["SelectionType"] = "MS",
                ["Message"] = prompt,
                ["Card"] = ability,
                ["CardKind"] = cardKind,
                ["Min"] = min,
                ["Max"] = max
            };
        }

        public static JObject FieldGateSelection(string prompt, int ability, int cardKind, params IEnumerable<GateCard> gates)
        {
            return new()
            {
                ["SelectionType"] = "GF",
                ["Message"] = prompt,
                ["Card"] = ability,
                ["CardKind"] = cardKind,
                ["SelectionGates"] = new JArray(gates.Select(x => new JObject
                {
                    ["Type"] = x.TypeId,
                    ["PosX"] = x.Position.X,
                    ["PosY"] = x.Position.Y,
                    ["CID"] = x.CardId
                }))
            };
        }

        public static JObject ActiveSelection(string message, int card, int kind, params IEnumerable<IActive> actives)
        {
            JArray jsonActives = [];

            foreach (IActive active in actives)
            {
                if (active is AbilityCard activeAbility)
                    jsonActives.Add(new JObject
                    {
                        ["Type"] = "C",
                        ["ActiveOwner"] = active.Owner.Id,
                        ["CardType"] = active.TypeId,
                        ["CardKind"] = (int)active.Kind,
                        ["CID"] = activeAbility.CardId,
                        ["EID"] = active.EffectId
                    });
                else
                    jsonActives.Add(new JObject
                    {
                        ["Type"] = "E",
                        ["ActiveOwner"] = active.Owner.Id,
                        ["CardType"] = active.TypeId,
                        ["CardKind"] = (int)active.Kind,
                        ["EID"] = active.EffectId
                    });
            }

            return new JObject
            {
                ["SelectionType"] = "AC",
                ["Message"] = message,
                ["Card"] = card,
                ["CardKind"] = kind,
                ["SelectionAbilities"] = jsonActives
            };
        }

        public static JObject GateSet(GateCard card, bool RevealInfo)
        {
            return new()
            {
                ["Type"] = "GateSetEvent",
                ["PosX"] = card.Position.X,
                ["PosY"] = card.Position.Y,
                ["GateData"] = new JObject
                {
                    ["Kind"] = (int)card.Kind,
                    ["Type"] = RevealInfo ? card.TypeId : -2
                },
                ["Owner"] = card.Owner.Id,
                ["CID"] = card.CardId
            };
        }

        public static JObject MultiGateSet((GateCard card, byte setBy)[] cards, byte forPlayer)
        {
            return new()
            {
                ["Type"] = "GateMultiSetEvent",
                ["Cards"] = JArray.FromObject(cards.Select(card => new JObject
                {
                    ["PosX"] = card.card.Position.X,
                    ["PosY"] = card.card.Position.Y,
                    ["GateData"] = new JObject
                    {
                        ["Kind"] = (int)card.card.Kind,
                        ["Type"] = card.setBy == forPlayer ? card.card.TypeId : -2
                    },
                    ["Owner"] = card.card.Owner.Id,
                    ["CID"] = card.card.CardId
                }))
            };
        }

        public static JObject GateRetracted(GateCard card, bool RevealInfo)
        {
            return new()
            {
                ["Type"] = "GateRetractedEvent",
                ["PosX"] = card.Position.X,
                ["PosY"] = card.Position.Y,
                ["GateData"] = new JObject
                {
                    ["Kind"] = (int)card.Kind,
                    ["Type"] = RevealInfo ? card.TypeId : -2
                },
                ["Owner"] = card.Owner.Id,
                ["CID"] = card.CardId
            };
        }

        public static JObject RemoveGate(GateCard card)
        {
            return new()
            {
                ["Type"] = "GateRemoved",
                ["PosX"] = card.Position.X,
                ["PosY"] = card.Position.Y,
            };
        }

        public static JObject SendGateToDrop(GateCard card) =>
            new()
            {
                ["Type"] = "GateSentToGrave",
                ["Kind"] = (int)card.Kind,
                ["CardType"] = card.TypeId,
                ["CID"] = card.CardId,
                ["Owner"] = card.Owner.Id
            };

        public static JObject SendAbilityToDrop(AbilityCard card) =>
            new()
            {
                ["Type"] = "AbilitySentToGrave",
                ["Kind"] = (int)card.Kind,
                ["CardType"] = card.TypeId,
                ["CID"] = card.CardId,
                ["Owner"] = card.Owner.Id
            };

        public static JObject AddMarkerToActiveZone(IActive active, bool isCopy) =>
            new()
            {
                ["Type"] = "EffectAddedActiveZone",
                ["IsCopy"] = isCopy,
                ["Card"] = active.TypeId,
                ["Kind"] = (int)active.Kind,
                ["Id"] = active.EffectId,
                ["User"] = active.User.BID,
                ["Owner"] = active.Owner.Id
            };

        public static JObject RemoveMarkerFromActiveZone(IActive active) =>
            new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = active.EffectId
            };

        public static JObject GateOpen(GateCard card)
        {
            return new()
            {
                ["Type"] = "GateOpenEvent",
                ["PosX"] = card.Position.X,
                ["PosY"] = card.Position.Y,
                ["GateData"] = new JObject
                {
                    ["Kind"] = (int)card.Kind,
                    ["Type"] = card.TypeId
                },
                ["Owner"] = card.Owner.Id,
                ["CID"] = card.CardId
            };
        }

        public static JObject GateRevealed(GateCard card)
        {
            return new()
            {
                ["Type"] = "GateRevealedEvent",
                ["PosX"] = card.Position.X,
                ["PosY"] = card.Position.Y,
                ["GateData"] = new JObject
                {
                    ["Kind"] = (int)card.Kind,
                    ["Type"] = card.TypeId
                },
                ["Owner"] = card.Owner.Id,
                ["CID"] = card.CardId
            };
        }

        public static JObject GateNegated(GateCard card)
        {
            return new JObject
            {
                ["Type"] = "GateNegateEvent",
                ["PosX"] = card.Position.X,
                ["PosY"] = card.Position.Y,
                ["Owner"] = card.Owner.Id,
                ["CID"] = card.CardId
            };
        }
    }
}
