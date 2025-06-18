using StowTown.Models;

namespace StowTown;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DashboardPage : ContentPage
{

	public DashboardPage()
	{
       // InitializeComponent();
        RadioStationItem();
        TodayCallList();
        MonthlySongList();
    }


    private void RadioStationItem()
    {
        using (var context = new StowTownDbContext())
        {
            var radiolist = context.RadioStations.Where(r => r.IsActive == true && r.IsDeleted != true).ToList();
            if (radiolist != null)
            {
                //NoRadioList.Text = radiolist.Count.ToString(); // Set the label for radio stations count
            }
        }
    }

    private void TodayCallList()
    {
        using (var context = new StowTownDbContext())
        {
            var callList = context.CallRecords.Where(c => c.CreatedAt == DateTime.Now.Date).ToList();
            if (callList != null)
            {
                //NoCallList = callList.Count();
                //NoOfCalls.Text = NoCallList.ToString(); // Set the label for call list count
            }
        }
    }

    private void MonthlySongList()
    {
        using (var context = new StowTownDbContext())
        {
            var songList = context.Songs.Where(s => s.CreatedAt.HasValue && s.CreatedAt.Value.Month == DateTime.Now.Month).ToList();
            if (songList != null)
            {
                //NoMonthList.Text = songList.Count.ToString(); // Set the label for song list count
            }
        }
    }

    public void LoadMonthlyTopData(DateTime date)
    {
        using (var context = new StowTownDbContext())
        {
            var monthlyTopData = context.MonthlySongLists.Where(m => m.Date.Value.Month == date.Month && m.Date.Value.Year == date.Year).ToList();
            if (monthlyTopData != null)
            {
                //NoMonthlyTopData = monthlyTopData.Count().ToString();
            }
        }
    }



}