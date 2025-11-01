using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Models;

namespace DualModeMonitorSystem.ViewModels
{
    public class MainViewModel
    {
        private readonly IRegionManager regionManager;

        
        public MainViewModel()
        {
            
        }
        public MainViewModel(IRegionManager regionManager)
        {
            
            this.regionManager = regionManager;
        }

        //public void NavigateTo(MenuItem item)
        //{
        //    regionManager.RequestNavigate("MainContentRegion", item.ViewName);
        //}
    }
}
