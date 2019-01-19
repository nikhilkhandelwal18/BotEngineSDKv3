using System;
using System.Collections.Generic;

namespace BotEngine.Models
{
    [Serializable]
    public class Benefit
    {
        public string Id { get; set; }
        public string BenefitDescription { get; set; }
    }

    [Serializable]
    public class BenefitsInformation
    {
        public bool BenefitsCovered { get; set; }
        public List<Benefit> Benefits { get; set; }
    }
}