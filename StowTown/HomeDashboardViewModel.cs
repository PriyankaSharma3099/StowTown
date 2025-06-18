using StowTown.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StowTown
{
    public class HomeDashboardViewModel 
    {
        private int _radioStationCount;
        private int _todayCallCount;
        private int _monthlySongCount;
        private bool _checked;

        public int RadioStationCount
        {
            get => _radioStationCount;
            set { _radioStationCount = value; OnPropertyChanged(); }
        }

        public int TodayCallCount
        {
            get => _todayCallCount;
            set { _todayCallCount = value; OnPropertyChanged(); }
        }

        public int MonthlySongCount
        {
            get => _monthlySongCount;
            set { _monthlySongCount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> MonthlyTopSongs { get; set; } = new ObservableCollection<string>();

        public bool Checked
        {
            get => _checked;
            set { _checked = value; OnPropertyChanged(); }
        }

        public HomeDashboardViewModel()
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            using (var context = new StowTownDbContext())
            {
                // Radio Station Count
                RadioStationCount = context.RadioStations.Count(r => r.IsActive == true && r.IsDeleted==false);

                // Today's Call Count
                TodayCallCount = context.CallRecords.Count(c => c.CreatedAt == DateTime.Now.Date);

                // Monthly Song Count
                MonthlySongCount = context.Songs.Count(s => s.CreatedAt.HasValue && s.CreatedAt.Value.Month == DateTime.Now.Month);

                // Monthly Top Songs
                var monthlyTopData = context.MonthlySongLists
                    .Where(m => m.Date.HasValue && m.Date.Value.Month == DateTime.Now.Month)
                    .Select(m =>m.FkSong)
                    .ToList();

                MonthlyTopSongs.Clear();
                foreach (var song in monthlyTopData)
                {
                    MonthlyTopSongs.Add(song.Value.ToString());
                }
            }
        }
    
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
