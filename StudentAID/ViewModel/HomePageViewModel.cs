using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using StudentAID.Services;

namespace StudentAID.ViewModel
{
    public partial class HomePageViewModel
    {
        public ICommand OnSwipe { get; set; }

        public HomePageViewModel()
        {
            OnSwipe = new Command<string>(async (route) => await Navigation.swipeNavigation(route));
        }
    }
}
