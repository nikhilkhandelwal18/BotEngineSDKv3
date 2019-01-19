using System;
using System.Collections.Generic;

namespace BotEngine.Models
{
    [Serializable]
    public class Plan
    {
        public string PlanId { get; set; }
        public string PlanDescription { get; set; }
        public string EnrollId { get; set; }

    }

    [Serializable]
    public class Cost
    {
        public string CostType { get; set; }
        public List<string> Values { get; set; }
    }
}