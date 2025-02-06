using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AB_Server.Gates
{
    internal class Intercept : GateCard
    {
        public Intercept(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            CardId = cID;
        }

        public override int TypeId { get; } = 11; // Assign a unique TypeId for Intercept

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(new()
                {
                    { "Type", "GateOpenEvent" },
                    { "PosX", Position.X },
                    { "PosY", Position.Y },
                    { "GateData", new JObject {
                        { "Type", TypeId } }
                    },
                    { "Owner", Owner.Id },
                    { "CID", CardId }
                });

            // Freeze battles on this Gate Card
            Freeze(this);

            // Subscribe to the GateAdded event to resume battles when another Gate Card is set on the field
            game.GateAdded += OnAnotherGateCardSet;

            game.CheckChain(Owner, this);
        }

        public override bool CheckBattles()
        {
            // Halt battles on this Gate Card until another Gate Card is set on the field
            return false;
        }

        private void OnAnotherGateCardSet(GateCard target, byte owner, params byte[] pos)
        {
            // Resume battles on this Gate Card when another Gate Card is set on the field
            if (target != this)
            {
                game.GateAdded -= OnAnotherGateCardSet;
                TryUnfreeze(this);
                base.CheckBattles();
            }
        }

        public override void Negate(bool asCounter = false)
        {
            base.Negate(asCounter);
            // Unfreeze battles and remove the event handler
            TryUnfreeze(this);
            game.GateAdded -= OnAnotherGateCardSet;
        }

        public override void Dispose()
        {
            // Remove the event handler when the gate card is removed from the field
            game.GateAdded -= OnAnotherGateCardSet;
            base.Dispose();
        }
    }
}


