using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks.Dataflow;

namespace AB_Server.Abilities
{
    internal class MarionetteEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Bakugan target;
        GateCard moveTarget;
        Game game { get => User.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public MarionetteEffect(Bakugan user, Bakugan target, GateCard moveTarget, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
            this.moveTarget = moveTarget;
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
                    { "Kind", 0 },
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

            target.Move(moveTarget);
        }
    }

    internal class Marionette : AbilityCard
    {
        public Marionette(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        Bakugan target;
        GateCard moveTarget;

        public override void Setup(bool asFusion)
        {
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_MOVETARGET", TypeId, (int)Kind, Game.BakuganIndex.Where(x => x.OnField() && x.Owner.SideID != Owner.SideID && x.Position != User.Position))
            ));

            Game.AwaitingAnswers[Owner.Id] = Setup3;
        }

        public void Setup3()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "GF" },
                        { "Message", "INFO_MOVETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => target.Position != x).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y },
                            { "CID", x.CardId }
                        })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public new void Activate()
        {
            moveTarget = Game.GateIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["gate"]];

            Game.CheckChain(Owner, this, User);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                new MarionetteEffect(User, target, moveTarget, TypeId, IsCopy).Activate();
            Dispose();
        }

        public override void DoubleEffect() =>
                new MarionetteEffect(User, target, moveTarget, TypeId, IsCopy).Activate();

        public new void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.OnField() && Game.BakuganIndex.Any(possibleTarget => possibleTarget.OnField() && possibleTarget.Position != user.Position && user.IsEnemyOf(possibleTarget));

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(possibleTarget => possibleTarget.OnField() && possibleTarget.Position != user.Position && user.IsEnemyOf(possibleTarget));
    }
}
