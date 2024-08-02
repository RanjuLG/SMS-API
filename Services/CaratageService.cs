using System;
using System.Collections.Generic;
using System.Linq;
using SMS.Interfaces;
using SMS.Models;

namespace SMS.Services
{
    public class CaratageService : ICaratageService
    {
        private readonly IRepository _dbContext;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public CaratageService(IRepository dbContext, IReadOnlyRepository readOnlyRepository)
        {
            _dbContext = dbContext;
            _readOnlyRepository = readOnlyRepository;
        }

        public IList<GoldCaratage> GetAllCaratages()
        {
            return _dbContext.Get<GoldCaratage>(g => g.DeletedAt == null).ToList();
        }

        public GoldCaratage GetCaratageById(int caratageId)
        {
            return _dbContext.Get<GoldCaratage>(g => g.CaratageId == caratageId && g.DeletedAt == null).FirstOrDefault();
        }

        public void CreateCaratage(GoldCaratage caratage)
        {
            _dbContext.Create<GoldCaratage>(caratage);
            _dbContext.Save();
        }

        public void UpdateCaratage(GoldCaratage caratage)
        {
            caratage.UpdatedAt = DateTime.Now;
            _dbContext.Update<GoldCaratage>(caratage);
            _dbContext.Save();
        }

        public void DeleteCaratage(int caratageId)
        {
            var caratage = _dbContext.GetById<GoldCaratage>(caratageId);
            if (caratage != null)
            {
                caratage.DeletedAt = DateTime.Now;
                _dbContext.Update<GoldCaratage>(caratage);
                _dbContext.Save();
            }
        }

        public void DeleteCaratages(IEnumerable<int> caratageIds)
        {
            var caratages = _dbContext.Get<GoldCaratage>(g => caratageIds.Contains(g.CaratageId) && g.DeletedAt == null).ToList();
            foreach (var caratage in caratages)
            {
                caratage.DeletedAt = DateTime.Now;
                _dbContext.Update<GoldCaratage>(caratage);
            }
            _dbContext.Save();
        }
    }
}
