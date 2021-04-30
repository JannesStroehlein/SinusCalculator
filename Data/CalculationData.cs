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

        public CalculationData(string Title)
        {
            this.Title = Title;
            this.Steps = new List<CalculationStep>();
        }
    }
}
