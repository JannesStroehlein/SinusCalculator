using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinusCalculator.Data
{
    /// <summary>
    /// Objekt um gleichzeitig Daten und Bereichnungsbeschreibungen zu übergeben
    /// </summary>
    /// <typeparam name="T">Der Typ der Daten</typeparam>
    public class CalculationResponse<T>
    {
        public T Data { get; private set; }
        public CalculationData Calculation { get; private set; }

        public CalculationResponse(T Data, CalculationData Calculation)
        {
            this.Data = Data;
            this.Calculation = Calculation;
        }
    }
}
