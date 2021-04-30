using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinusCalculator.Data
{
    public class CalculationStep
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Calculation { get; set; }

        public CalculationStep(string Title, string Description, string Calculation)
        {
            this.Title = Title;
            this.Description = Description;
            this.Calculation = Calculation;
        }
    }
}
