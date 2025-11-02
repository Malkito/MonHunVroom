public static class MonsterStates
{
    public const string WANDER = "monster_wander";
    public const string ATTACK = "monster_attack";
    public const string DEAD = "monster_dead";
    public const string RESPAWN = "monster_respawn";

    public static class CreatePaths
    {
        private const string CREATE_PATH_DIRECTORY = "Monster/States/";

        public const string WANDER = CREATE_PATH_DIRECTORY + MonsterStates.WANDER;
        public const string ATTACK = CREATE_PATH_DIRECTORY + MonsterStates.ATTACK;
        public const string DEAD = CREATE_PATH_DIRECTORY + MonsterStates.DEAD;
        public const string RESPAWN = CREATE_PATH_DIRECTORY + MonsterStates.RESPAWN;
    }
}
