namespace OutbreakZCore.Client.Core.Zombie
{
    public class ZombieSettings
    {
        public string ZombieSkin { get; set; }
        public float WalkSpeed { get; set; }
        public float RunSpeed { get; set; }
        public int MaxHealth { get; set; }

        public ZombieSettings(string skin, int maxHealth, float walkSpeed, float runSpeed)
        {
            ZombieSkin = skin;
            MaxHealth = maxHealth;
            WalkSpeed = walkSpeed;
            RunSpeed = runSpeed;
        }
    }
}