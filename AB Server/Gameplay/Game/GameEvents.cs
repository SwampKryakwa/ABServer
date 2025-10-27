using Newtonsoft.Json.Linq;

namespace AB_Server;

internal partial class Game
{
    public void ThrowEvent(JObject @event, params int[] exclude)
    {
        Console.WriteLine(@event);
        for (int i = 0; i < PlayerCount; i++)
            if (!exclude.Contains(i))
                NewEvents[i].Add(@event);
        foreach (var spectator in Spectators)
            SpectatorEvents[spectator].Add(@event);
    }

    public void ThrowEvent(int reciever, JObject @event)
    {
        Console.WriteLine(@event);
        NewEvents[reciever].Add(@event);
    }

    public JArray GetEvents(int player)
    {
        JArray toReturn;
        toReturn = [.. NewEvents[player]];
        NewEvents[player].Clear();

        return toReturn;
    }

    public JArray GetSpectatorEvents(long uuid)
    {
        JArray toReturn;
        toReturn = [.. SpectatorEvents[uuid]];
        SpectatorEvents[uuid].Clear();

        return toReturn;
    }
}