namespace Scp035.EventHandlers
{
    using MEC;

    public static class ServerHandlers
    {
        internal static void OnRoundStarted()
        {
            Methods.IsRotating = true;
            Methods.ScpPickups.Clear();
            
            Methods.CoroutineHandles.Add(Timing.RunCoroutine(Methods.RunSpawning()));
            Methods.CoroutineHandles.Add(Timing.RunCoroutine(Methods.CorrodePlayers()));
        }
        
        internal static void OnWaitingForPlayers()
        {
            Timing.KillCoroutines(Methods.CoroutineHandles.ToArray());
            Methods.CoroutineHandles.Clear();
        }
    }
}