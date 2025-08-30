using AB_Server.Abilities;
using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Warlock : GateCard
    {
        public Warlock(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 2;

        Bakugan targetedBakugan;

        public override void Open()
        {
            // Find opponent's Bakugan on this card
            var targetables = Bakugans.Where(x => x.Owner != Owner).ToArray();
            if (targetables.Length == 0)
            {
                game.ChainStep();
                return;
            }

            // Prompt to select an opponent Bakugan on this card
            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                EventBuilder.FieldBakuganSelection("INFO_GATE_TARGET", TypeId, (int)Kind, targetables)
            ));
            game.OnAnswer[Owner.Id] = () =>
            {
                targetedBakugan = game.BakuganIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];
                game.CheckChain(Owner, this);
            };
        }

        public override void Resolve()
        {
            if (Negated || targetedBakugan is null || targetedBakugan.Position != this)
            {
                game.ChainStep();
                return;
            }

            // Present the two options to the owner
            game.ThrowEvent(Owner.Id, EventBuilder.OptionSelectionEvent("INFO_PICKER_WARLOCK", 2));
            game.OnAnswer[Owner.Id] = () =>
            {
                int option = (int)game.PlayerAnswers[Owner.Id]!["option"];
                if (option == 0)
                {
                    // Set power to base power
                    targetedBakugan.Boost((short)(targetedBakugan.BasePower - targetedBakugan.Power), this);
                }
                else
                {
                    // Remove all markers created by the target
                    foreach (var effect in game.ActiveZone.Where(x=>x.User == targetedBakugan))
                    {
                        effect.Negate();
                        game.ActiveZone.Remove(effect);
                    }
                }
                game.ChainStep();
            };
        }

        public override bool IsOpenable() => Bakugans.Any(x=>x.Owner != Owner) && base.IsOpenable();
    }
}
