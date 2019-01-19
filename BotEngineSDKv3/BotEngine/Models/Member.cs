using System;

namespace BotEngine.Models
{
    [Serializable]
    public class Member
    {
        public string Id { get; set; }
        public string fullname { get; set; }
        public Int16 age { get; set; }
        public string gender { get; set; }

        public Plan activePlan { get; set; }
    }
}