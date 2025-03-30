using System;
using System.Linq;
using System.Collections.Generic;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class Marionette : FusionAbility
    {
        public Marionette(int cID, Player owner)
        {
            TypeId = 8;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(SlingBlazer);
        }

        Bakugan user;           // Выбранный Мантис (пользователь)
        Bakugan targetBakugan;    // Выбранный бакуган противника
        GateCard targetGate;      // Выбранная карта ворот для перемещения

        // Этап 1. Выбор Мантиса из бакуганов игрока
        public override void PickUser()
        {
            var validUsers = Owner.BakuganOwned.Where(b => b.Type == BakuganType.Mantis && b.OnField());
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_MARIANETTE_USER", TypeId, (int)Kind, validUsers)
            ));
            Game.AwaitingAnswers[Owner.Id] = PickTarget;
        }

        // Этап 2. Выбор бакугана оппонента, не находящегося на той же карте ворот, что и выбранный Мантис
        public void PickTarget()
        {
            user = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            var validTargets = Game.BakuganIndex.Where(b =>
                b.Owner != Owner &&
                b.OnField() &&
                (b.Position is GateCard targetGate && !(user.Position is GateCard userGate && userGate == targetGate))
            );
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_MARIANETTE_TARGET", TypeId, (int)Kind, validTargets)
            ));
            Game.AwaitingAnswers[Owner.Id] = PickGate;
        }

        // Этап 3. Выбор карты ворот, отличной от той, на которой находится выбранный бакуган
        public void PickGate()
        {
            targetBakugan = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            GateCard currentGate = targetBakugan.Position as GateCard;
            var validGates = Game.GateIndex.Where(g => g != currentGate);
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldGateSelection("INFO_MARIANETTE_GATE", TypeId, (int)Kind, validGates)
            ));
            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        // Завершает выбор, начиная цепочку эффектов
        public new void Activate()
        {
            targetGate = Game.GateIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["gate"]];
            Game.CheckChain(Owner, this, user);
        }

        // Вызов эффекта способности, реализованного в отдельном классе
        public override void Resolve()
        {
            if (!counterNegated)
                new MarionetteEffect(user, targetBakugan, targetGate, TypeId).Activate();
            Dispose();
        }

        public override void DoubleEffect() =>
            new MarionetteEffect(user, targetBakugan, targetGate, TypeId).Activate();
    }

    internal class MarionetteEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan targetBakugan;
        GateCard targetGate;
        Game game { get => targetBakugan.Game; }

        public MarionetteEffect(Bakugan user, Bakugan targetBakugan, GateCard targetGate, int typeID)
        {
            this.user = user;
            this.targetBakugan = targetBakugan;
            this.targetGate = targetGate;
            TypeId = typeID;
        }

        // Эффект перемещения выбранного бакугана на выбранную карту ворот.
        public void Activate()
        {
            // Рассылаем сообщение об активации эффекта всем игрокам
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "FusionAbilityActivateEffect" },
                    { "Kind", 1 },
                    { "Card", TypeId },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Treatment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            // Перемещаем бакугана на выбранную карту ворот, если он все еще на поле
            if (targetBakugan.OnField())
                targetBakugan.Move(targetGate, MoveSource.Effect);
        }
    }
}
