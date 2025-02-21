using OutbreakZCore.Client.Core;

namespace OutbreakZCore.Client.Config
{
    public static class ClientConfig
    {
        #region Zombie
        public const int ZombieContextTickDelay = 500;

        public const int ZombieDamage = 10;
        public const float ZombieAttackAngleThreshold = 60.0f;
        public const float ZombieDistanceToRun = 30.0f;
        public const float ZombieDistanceToMelee = 2.5f;
        public const float ZombieDistanceToTakeDamage = 1.3f;
        public const int ZombieDelayBetweenAttacks = 2500;
        
        public const string ZombieMoveSet = "move_m@drunk@verydrunk";
        public const string ZombieAttackDictionary = "melee@unarmed@streamed_core_fps";
        public const string ZombieAttackAnim = "ground_attack_0_psycho";

        public const int MaxOwnZombies = 5;
        public const int ZombieSpawnDelay = 1500;
        public const float SpawnZombieMinRadius = 30.0f;
        public const float SpawnZombieMaxRadius = 60.0f;

        public const float ZombieNearestPlayerScanRadius = 200.0f;

        public static readonly ZombieSpawnManager.ZombieSettings[] ZombiePresets = {
            // male
            new ZombieSpawnManager.ZombieSettings("a_m_m_hillbilly_01", 500, 0.5f, 1.5f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_hillbilly_02", 500, 0.5f, 1.5f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_skidrow_01", 400, 0.4f, 1.4f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_tramp_01", 450, 0.45f, 1.45f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_trampbeac_01", 420, 0.42f, 1.42f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_acult_01", 600, 0.6f, 1.6f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_og_boss_01", 700, 0.65f, 1.7f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_soucent_03", 480, 0.48f, 1.48f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_hasjew_01", 460, 0.46f, 1.46f),
            new ZombieSpawnManager.ZombieSettings("a_m_m_tranvest_01", 500, 0.5f, 1.5f),
            new ZombieSpawnManager.ZombieSettings("a_m_y_methhead_01", 550, 0.55f, 1.55f),
            new ZombieSpawnManager.ZombieSettings("a_m_y_hippy_01", 520, 0.52f, 1.52f),
            new ZombieSpawnManager.ZombieSettings("a_m_y_skater_01", 530, 0.53f, 1.53f),
            new ZombieSpawnManager.ZombieSettings("a_m_y_juggalo_01", 580, 0.58f, 1.58f),
            new ZombieSpawnManager.ZombieSettings("a_m_y_soucent_04", 490, 0.49f, 1.49f),
            new ZombieSpawnManager.ZombieSettings("u_m_m_prolsec_01", 600, 0.6f, 1.6f),
            new ZombieSpawnManager.ZombieSettings("a_m_o_acult_01", 620, 0.62f, 1.62f),
            new ZombieSpawnManager.ZombieSettings("a_m_o_acult_02", 630, 0.63f, 1.63f),
            new ZombieSpawnManager.ZombieSettings("a_m_y_acult_01", 610, 0.61f, 1.61f),
            new ZombieSpawnManager.ZombieSettings("a_m_y_acult_02", 590, 0.59f, 1.59f),

            // female
            new ZombieSpawnManager.ZombieSettings("a_f_m_trampbeac_01", 400, 0.4f, 1.4f),
            new ZombieSpawnManager.ZombieSettings("a_f_m_tramp_01", 420, 0.42f, 1.42f),
            new ZombieSpawnManager.ZombieSettings("a_f_m_skidrow_01", 430, 0.43f, 1.43f),
            new ZombieSpawnManager.ZombieSettings("a_f_y_hippie_01", 450, 0.45f, 1.45f),
            new ZombieSpawnManager.ZombieSettings("a_f_y_skater_01", 470, 0.47f, 1.47f)
        };
        #endregion
        
         
        #region GameMode
        public const int GameModeFirstSpawnDelayMs = 1000;
        public const float GameModeSpawnThreshold = 20.0f;
        public const int GameModeRespawnCheckerTickDelay = 1000;
        #endregion
    }
}