using StowTown.Pages.CallHistory;
using StowTown.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown
{
    public class UserClickBehavior : Behavior<CheckBox>
    {
        protected override void OnAttachedTo(CheckBox checkBox)
        {
            base.OnAttachedTo(checkBox);
            checkBox.CheckedChanged += OnCheckedChanged;
        }

        protected override void OnDetachingFrom(CheckBox checkBox)
        {
            base.OnDetachingFrom(checkBox);
            checkBox.CheckedChanged -= OnCheckedChanged;
        }

        private void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null && e.Value) // CheckBox was checked  
            {
                var station = cb.BindingContext as CallRecordViewModel;
                if (station != null)
                {
                    Shell.Current.GoToAsync($"{nameof(CreateCallHistory)}?SelectedId={station.Id}&Type=Create");
                }
            }
        }

    }

}
