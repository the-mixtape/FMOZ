using CitizenFX.Core;

namespace OutbreakZCore.Shared
{
    /// <summary>
    /// Represents spawn information, including coordinates and heading for an entity.
    /// </summary>
    public class SpawnPosition
    {
        /// <summary>
        /// Gets or sets of the spawn location.
        /// </summary>
        public Vector3 Location { get; set; }
        
        /// <summary>
        /// Gets or sets the heading (rotation) of the entity at the spawn location, in degrees.
        /// </summary>
        public float Heading { get; set; }

        public SpawnPosition()
        {
            this.Location = Vector3.Zero;
            this.Heading = 0f;
        }

        public SpawnPosition(float x, float y, float z, float heading)
        {
            this.Location = new Vector3(x, y, z);
            this.Heading = heading;
        }
        
        public override string ToString()
        {
            return $"({this.Location.X}, {this.Location.Y}, {this.Location.Z})";
        }
    }
}