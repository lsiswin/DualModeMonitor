using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Services
{
    public interface IHttpService
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task<T> DeleteAsync<T>(string endpoint);
        void SetBaseAddress(string baseAddress);
        void SetAuthenticationToken(string token);
    }

}
