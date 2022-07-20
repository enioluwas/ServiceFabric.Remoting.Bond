using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceFabric.Remoting.Bond.Test
{
    [global::Bond.Schema]
    internal class BondGameInfo
    {
        [global::Bond.Id(0), global::Bond.Required]
        public List<string> PlayerNames { get; set; }

        [global::Bond.Id(1), global::Bond.Required]
        public string ServerName { get; set; }

        [global::Bond.Id(2), global::Bond.Required, global::Bond.Type(typeof(long))]
        public TimeSpan Duration { get; set; }

        public BondGameInfo()
        {
            this.PlayerNames = new List<string>();
            this.ServerName = "";
            this.Duration = new TimeSpan();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BondGameInfo);
        }

        public bool Equals(BondGameInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Duration == other.Duration
                && this.ServerName == other.ServerName
                && this.PlayerNames.SequenceEqual(other.PlayerNames);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.ServerName, this.Duration, this.PlayerNames);
        }
    }
}
