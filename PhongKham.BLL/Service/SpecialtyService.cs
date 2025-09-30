using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhongKham.DAL.Entities;

namespace PhongKham.BLL.Service
{
    public class SpecialtyService
    {
        // Giả lập DB bằng bộ nhớ
        private static List<Specialty> _specialties = new List<Specialty>
        {
            new Specialty { SpecialtyId = 1, SpecialtyName = "Nội khoa" },
            new Specialty { SpecialtyId = 2, SpecialtyName = "Ngoại khoa" },
            new Specialty { SpecialtyId = 3, SpecialtyName = "Nhi khoa" }
        };

        public List<Specialty> GetAll()
        {
            return _specialties;
        }

        public Specialty? GetById(int id)
        {
            return _specialties.FirstOrDefault(s => s.SpecialtyId == id);
        }

        public void Add(Specialty specialty)
        {
            specialty.SpecialtyId = _specialties.Any() ? _specialties.Max(s => s.SpecialtyId) + 1 : 1;
            _specialties.Add(specialty);
        }

        public void Update(Specialty specialty)
        {
            var existing = _specialties.FirstOrDefault(s => s.SpecialtyId == specialty.SpecialtyId);
            if (existing != null)
            {
                existing.SpecialtyName = specialty.SpecialtyName;
            }
        }

        public void Delete(int id)
        {
            var specialty = _specialties.FirstOrDefault(s => s.SpecialtyId == id);
            if (specialty != null)
            {
                _specialties.Remove(specialty);
            }
        }
    }
}

