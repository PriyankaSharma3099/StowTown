using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.HelperService
{
    public class ShellHelper
    {
        public static void UpdateFlyoutItemTitle(string route, string newTitle, string newIcon = null)
        {
            if (Application.Current.MainPage is AppShell appShell)
            {
                var item = appShell.Items.FirstOrDefault(i => i.Route == route);
                if (item is FlyoutItem flyoutItem)
                {
                    flyoutItem.Title = newTitle;

                    if (!string.IsNullOrEmpty(newIcon))
                        flyoutItem.Icon = newIcon;
                }
            }
        }


    }
}
