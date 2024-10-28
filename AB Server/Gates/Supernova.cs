using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Supernova : GateCard, IGateCard
    {
        bool effectTriggered = false;

        public Supernova(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 8;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;

            game.BakuganMoved -= OnBakuganMove;
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganPlacedFromGrave -= OnBakuganStands;
            game.BakuganReturned -= OnBakuganLeaves;
            game.BakuganDestroyed -= OnBakuganLeaves;
        }

        public new void Open()
        {
            base.Open();

            foreach (var bakugan in Bakugans)
            {
                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganBoostedEvent" },
                        { "Owner", bakugan.Owner.Id },
                        { "Boost", -(bakugan.Power * 2) },
                        { "Bakugan", new JObject {
                            { "Type", (int)bakugan.Type },
                            { "Attribute", (int)bakugan.Attribute },
                            { "Treatment", (int)bakugan.Treatment },
                            { "Power", bakugan.Power },
                            { "BID", bakugan.BID } }
                        }
                    });
                }
                bakugan.PowerModifier *= -1;
                bakugan.affectingEffects.Add(this);
            }

            game.BakuganMoved += OnBakuganMove;
            game.BakuganThrown += OnBakuganStands;
            game.BakuganPlacedFromGrave += OnBakuganStands;
            game.BakuganReturned += OnBakuganLeaves;
            game.BakuganDestroyed += OnBakuganLeaves;
        }

        public void OnBakuganMove(Bakugan target, BakuganContainer pos)
        {
            if (pos == this)
            {
                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganBoostedEvent" },
                        { "Owner", target.Owner.Id },
                        { "Boost", -(target.Power * 2) },
                        { "Bakugan", new JObject {
                            { "Type", (int)target.Type },
                            { "Attribute", (int)target.Attribute },
                            { "Treatment", (int)target.Treatment },
                            { "Power", target.Power },
                            { "BID", target.BID } }
                        }
                    });
                }
                target.PowerModifier *= -1;
                target.affectingEffects.Add(this);
            }
            else if (target.affectingEffects.Contains(this))
            {
                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganBoostedEvent" },
                        { "Owner", target.Owner.Id },
                        { "Boost", -(target.Power * 2) },
                        { "Bakugan", new JObject {
                            { "Type", (int)target.Type },
                            { "Attribute", (int)target.Attribute },
                            { "Treatment", (int)target.Treatment },
                            { "Power", target.Power },
                            { "BID", target.BID } }
                        }
                    });
                }
                target.PowerModifier *= -1;
                target.affectingEffects.Remove(this);
            }
        }

        public void OnBakuganStands(Bakugan target, ushort owner, BakuganContainer pos)
        {
            if (pos == this)
            {
                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganBoostedEvent" },
                        { "Owner", target.Owner.Id },
                        { "Boost", -(target.Power * 2) },
                        { "Bakugan", new JObject {
                            { "Type", (int)target.Type },
                            { "Attribute", (int)target.Attribute },
                            { "Treatment", (int)target.Treatment },
                            { "Power", target.Power },
                            { "BID", target.BID } }
                        }
                    });
                }
                target.PowerModifier *= -1;
                target.affectingEffects.Add(this);
            }
            else if (target.affectingEffects.Contains(this))
            {
                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganBoostedEvent" },
                        { "Owner", target.Owner.Id },
                        { "Boost", -(target.Power * 2) },
                        { "Bakugan", new JObject {
                            { "Type", (int)target.Type },
                            { "Attribute", (int)target.Attribute },
                            { "Treatment", (int)target.Treatment },
                            { "Power", target.Power },
                            { "BID", target.BID } }
                        }
                    });
                }
                target.PowerModifier *= -1;
                target.affectingEffects.Remove(this);
            }
        }

        public void OnBakuganLeaves(Bakugan target, ushort owner)
        {
            if (target.affectingEffects.Contains(this))
            {
                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganBoostedEvent" },
                        { "Owner", target.Owner.Id },
                        { "Boost", -(target.Power * 2) },
                        { "Bakugan", new JObject {
                            { "Type", (int)target.Type },
                            { "Attribute", (int)target.Attribute },
                            { "Treatment", (int)target.Treatment },
                            { "Power", target.Power },
                            { "BID", target.BID } }
                        }
                    });
                }
                target.PowerModifier *= -1;
                target.affectingEffects.Remove(this);
            }
        }
    }
}
