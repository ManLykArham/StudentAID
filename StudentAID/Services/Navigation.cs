using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StudentAID.Services
{
    public partial class Navigation
    {
        public static async Task swipeNavigation(string route)
        {
            try
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(100));
                // Navigate to ChatBotPage
                await Shell.Current.GoToAsync(route);
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

