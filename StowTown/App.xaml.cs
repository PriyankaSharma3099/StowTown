namespace StowTown
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                File.WriteAllText(filePath, "Set Up Success");
                MainPage = new MainPage();

                Routing.RegisterRoute("CreateProjectProducer", typeof(CreateProjectProducer));
            }
            catch (Exception ex)
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                File.WriteAllText(filePath, ex.ToString());


            }


        }

    }
}
    