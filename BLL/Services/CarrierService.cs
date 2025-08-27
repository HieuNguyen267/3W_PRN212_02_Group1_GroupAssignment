using System.Collections.Generic;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services
{
    public class CarrierService
    {
        private readonly CarrierRepo _carrierRepo;

        public CarrierService()
        {
            _carrierRepo = new CarrierRepo();
        }

        public List<Carrier> GetAllCarriers()
        {
            return _carrierRepo.GetAllCarriers();
        }

        public Carrier? GetCarrierById(int carrierId)
        {
            return _carrierRepo.GetCarrierById(carrierId);
        }

        public bool AddCarrier(Carrier carrier, Account account)
        {
            return _carrierRepo.AddCarrier(carrier, account);
        }

        public bool UpdateCarrier(Carrier carrier)
        {
            return _carrierRepo.UpdateCarrier(carrier);
        }

        public bool DeleteCarrier(int carrierId)
        {
            return _carrierRepo.DeleteCarrier(carrierId);
        }

        public bool UpdateCarrierAvailable(int carrierId, bool isAvailable)
        {
            return _carrierRepo.UpdateCarrierAvailable(carrierId, isAvailable);
        }

        public bool UpdateAccountStatus(int accountId, bool isActive)
        {
            return _carrierRepo.UpdateAccountStatus(accountId, isActive);
        }

        public bool UsernameOrEmailExists(string username, string email)
        {
            return _carrierRepo.UsernameOrEmailExists(username, email);
        }

        public List<Carrier> GetAvailableCarriers()
        {
            return _carrierRepo.GetAvailableCarriers();
        }
    }
}


