using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.ViewModels
{
    public class WeekDaysViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public enum Weekday
        {             
            Monday = 1,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday
        }

    }
}
