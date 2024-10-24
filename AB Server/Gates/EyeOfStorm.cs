using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class EyeOfStorm : GateCard, IGateCard
    {
        Dictionary<Bakugan, Attribute> affectedBakugan;

        public EyeOfStorm(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 5;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
        }

        public new void Set(int posX, int posY)
        {
            game.BakuganThrown += OnBakuganStands;
            game.BakuganAdded += OnBakuganStands;

            base.Set(posX, posY);
        }

        public new void Remove()
        {
            foreach (var bakugan in affectedBakugan.Keys)
                bakugan.ChangeAttribute(affectedBakugan[bakugan], this);

            game.BakuganThrown -= OnBakuganStands;
            game.BakuganAdded -= OnBakuganStands;

            base.Remove();
        }

        Bakugan target;

        public void OnBakuganStands(Bakugan target, ushort owner, BakuganContainer pos)
        {
            if (OpenBlocking.Count != 0)
                return;
            
            if (IsTouching(pos as GateCard))
            {
                this.target = target;
                Open();
            }
        }

        public new void Open()
        {
            game.BakuganThrown -= OnBakuganStands;
            game.BakuganAdded -= OnBakuganStands;
            game.DontThrowTurnStartEvent = true;

            base.Open();

            game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "GF" },
                        { "Message", "INFO_MOVETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(game.GateIndex.Where(x => this.IsTouching(x as GateCard)).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y },
                            { "CID", x.CardId }
                        })) }
                    }
                } }
            });

            game.awaitingAnswers[Owner.Id] = Resolve;
        }

        public void Resolve()
        {
            target.Move(game.GateIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["gate"]] as GateCard);

            game.ContinueGame();
        }

        public bool IsOpenable() =>
            false;
    }
}
