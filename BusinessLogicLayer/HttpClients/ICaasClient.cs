using BusinessLogicLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.HttpClients
{
    public interface ICaasClient
    {
        Task<List<CaasResponse>?> FetchKitties();
        Task<byte[]?> DownloadImageAsync(string url);
    }

}
