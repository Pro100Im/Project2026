namespace Code.Infrastructure.Identifiers
{
    public static class EntityIdentifier
    {
        private static int _lastId = 1;

        public static int Next() => ++_lastId;

        public static void Reset() => _lastId = 1;
    }
}