using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class BigBrawl : GateCard, IGateCard
    {
        public BigBrawl(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 3;

        public int PlayersAnswered;
        public List<Bakugan> targets;

        public new void Open()
        {
            PlayersAnswered = 0;
            targets = new List<Bakugan>();

            base.Open();

            foreach (var playerEvents in game.NewEvents)
                playerEvents.Add(EventBuilder.SelectionBundler(EventBuilder.BoolSelectionEvent("BIGBRAWL_ADDBAKUGAN")));

            for (int i = 0; i < game.AwaitingAnswers.Length; i++)
                game.AwaitingAnswers[i] = () => CheckPlayerAddsBakugan(i);
        }

        public void CheckPlayerAddsBakugan(int playerId)
        {
            if (!(bool)game.IncomingSelection[playerId]["array"][0]["answer"])
            {
                PlayersAnswered++;
            }
            else
            {
                game.NewEvents[playerId].Add(new JObject {
                    { "Type", "StartSelection" },
                    { "Selections", new JArray {
                        new JObject {
                            { "SelectionType", "BH" },
                            { "Message", "INFO_GATE_ADDTARGET" },
                            { "Ability", TypeId },
                            { "SelectionBakugans", new JArray(Bakugans.Select(x =>
                                new JObject { { "Type", (int)x.Type },
                                    { "Attribute", (int)x.Attribute },
                                    { "Treatment", (int)x.Treatment },
                                    { "Power", x.Power },
                                    { "Owner", x.Owner.Id },
                                    { "BID", x.BID } })) }
                        }
                    } }
                });

                game.AwaitingAnswers[playerId] = () => AddPlayersBakugan(playerId);
            }

            if (PlayersAnswered == game.PlayerCount)
                Resolve();
        }

        public void AddPlayersBakugan(int playerId)
        {
            PlayersAnswered++;

            targets.Add(game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]]);

            if (PlayersAnswered == game.PlayerCount)
                Resolve();
        }

        public void Resolve()
        {
            Bakugan.MultiAdd(game, this, MoveSource.Effect, targets.ToArray());
            game.ContinueGame();
        }
    }
}
