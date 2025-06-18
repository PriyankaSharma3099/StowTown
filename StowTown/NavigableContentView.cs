using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown
{
    public class NavigableContentView : ContentView
    {
        public Action<ContentView>? NavigateTo;
        public Action? NavigateBack;
    }
}
