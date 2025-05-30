public static class MonsterStates
{
    public const string WANDER = "Wander";
    public const string RAMPAGE = "Rampage";
    public const string TARGET_PLAYER = "Assault";
    public const string DEAD = "Dead";
    public const string RESPAWN = "Respawn";

    public static class CreatePaths
    {
        private const string CREATE_PATH_DIRECTORY = "Monster/States/";

        public const string WANDER = CREATE_PATH_DIRECTORY + MonsterStates.WANDER;
        public const string RAMPAGE = CREATE_PATH_DIRECTORY + MonsterStates.RAMPAGE;
        public const string TARGET_PLAYER = CREATE_PATH_DIRECTORY + MonsterStates.TARGET_PLAYER;
        public const string DEAD = CREATE_PATH_DIRECTORY + MonsterStates.DEAD;
        public const string RESPAWN = CREATE_PATH_DIRECTORY + MonsterStates.RESPAWN;
    }
}
