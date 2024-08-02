using System.Collections.Generic;
using SMS.Models;

namespace SMS.Interfaces
{
    public interface ICaratageService
    {
        IList<GoldCaratage> GetAllCaratages();
        GoldCaratage GetCaratageById(int caratageId);
        void CreateCaratage(GoldCaratage caratage);
        void UpdateCaratage(GoldCaratage caratage);
        void DeleteCaratage(int caratageId);
        void DeleteCaratages(IEnumerable<int> caratageIds);
    }
}
