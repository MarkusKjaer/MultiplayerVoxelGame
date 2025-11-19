using System.Collections.Concurrent;

namespace CubeEngine.Engine.Client.Graphics
{
    public static class GLActionQueue
    {
        private static readonly ConcurrentQueue<Action> queue = new();

        public static void Enqueue(Action action)
        {
            if (action == null) return;
            queue.Enqueue(action);
        }

        public static void ProcessAll()
        {
            while (queue.TryDequeue(out var action))
            {
                try { action(); }
                catch (Exception ex)
                {
                    Console.WriteLine($"GLAction exception: {ex}");
                }
            }
        }
    }
}
