using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace AB_Server.Abilities
{
    internal class PlateauEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;

        public Player Owner { get => User.Owner; }
        bool IsCopy;

        public PlateauEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            Console.WriteLine(user);
            Console.WriteLine(user.Position);
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            foreach (GateCard gate in game.GateIndex)
                gate.OpenBlocking.Add(this);

            game.BakuganReturned += OnBakuganLeaves;
            game.BakuganDestroyed += OnBakuganLeaves;
        }

        public void OnBakuganLeaves(Bakugan target, ushort owner)
        {
            if (!game.BakuganIndex.Any(x => x.OnField() && x.Attribute == Attribute.Subterra))
            {
                foreach (GateCard gate in game.GateIndex)
                    gate.OpenBlocking.Remove(this);

                game.BakuganReturned -= OnBakuganLeaves;
                game.BakuganDestroyed -= OnBakuganLeaves;
            }
        }
    }

    internal class Plateau : AbilityCard, IAbilityCard
    {
        public Plateau(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new PlateauEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
            new PlateauEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.Attribute == Attribute.Subterra && user.OnField();
    }
}
