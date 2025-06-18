using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using NSubstitute.Routing;

namespace StowTown.Services
{


    public class DIPageRouteFactory<TPage> : Microsoft.Maui.Controls.RouteFactory where TPage : Page
    {
        public override Element GetOrCreate()
        {
            var page = App.Current?.Handler?.MauiContext?.Services?.GetService(typeof(TPage)) as Element;

            if (page == null)
            {
                throw new InvalidOperationException($"Could not resolve page {typeof(TPage).Name} from DI container.");
            }

            return page;
        }

        public override Element GetOrCreate(IServiceProvider services)
        {
            var page = services.GetService(typeof(TPage)) as Element;

            if (page == null)
            {
                throw new InvalidOperationException($"Could not resolve page {typeof(TPage).Name} from provided IServiceProvider.");
            }

            return page;
        }
    }
}
