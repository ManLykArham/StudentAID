using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentAID.Model
{
    public partial class Library : ObservableObject
    {
        [ObservableProperty]
        private string libraryName;

        [ObservableProperty]
        private double libraryDistance;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
