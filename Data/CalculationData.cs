using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinusCalculator.Data
{
    public class CalculationData
    {
        public List<CalculationStep> Steps { get; }
        public string Title { get; private set; }
        public string Description { get; private set; }

        public CalculationData(string Title, string Description)
        {
            this.Title = Title;
            this.Description = Description;
            this.Steps = new List<CalculationStep>();
        }
    }
}
