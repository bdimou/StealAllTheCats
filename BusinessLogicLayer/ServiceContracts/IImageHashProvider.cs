using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.ServiceContracts
{
    public interface IImageHashProvider
    {
        string ComputeHash(byte[] imageBytes);
    }
}
