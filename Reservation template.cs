MainView.Xaml

<TabItem Header="Reserve Tickets">
    <views:ReservationView DataContext="{Binding ReservationViewModel}" />
</TabItem>

App.Xaml.cs

var reservationRepo = new RepositoryJson<Reservation>("reservations.json");
new ReservationViewModel(screeningRepo, reservationRepo, dialogService);

MainViewModel

, ReservationViewModel reservationVm
ReservationViewModel = reservationVm;

ReservationView.xaml

<UserControl x:Class="MoviesG5.UI.ReservationView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        mc:Ignorable="d">
    <Grid>
        <TextBlock Text="Ticket reservation coming soon" HorizontalAlignment="Center" VerticalAlignment="Center" FontSize="18"/>
    </Grid>
</UserControl>

ReservationView.xaml.cs

using System.Windows.Controls;

namespace MoviesG5.UI
{
    public partial class ReservationView : UserControl
    {
        public ReservationView()
        {
            InitializeComponent();
        }
    }
}

ReservationViewModel:

using MoviesG5.Core;

namespace MoviesG5.UI
{
    public class ReservationViewModel : ViewModelBase
    {
        private readonly IRepository<Screening> _screeningRepo;
        private readonly IRepository<Reservation> _reservationRepo;
        private readonly IDialogService _dialogService;

        public ReservationViewModel(IRepository<Screening> screeningRepo, IRepository<Reservation> reservationRepo, IDialogService dialogService)
        {
            _screeningRepo = screeningRepo;
            _reservationRepo = reservationRepo;
            _dialogService = dialogService;
        }
    }
}

WelcomeViewModel.cs:

public ReservationViewModel ReservationViewModel { get; }

Reservation.cs

using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.Core
{
    public class Reservation : IHasId
    {
        public int Id { get; set; }
    }
}
