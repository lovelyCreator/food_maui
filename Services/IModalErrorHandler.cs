using System;
using System.Threading.Tasks;

namespace Food_maui.Services
{
    public interface IModalErrorHandler
    {
        Task HandleError(Exception ex);
    }
}