using Microsoft.EntityFrameworkCore;
using StowTown.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.Services
{
    public class RadioStationService
    {
            private readonly DbContext _context; // Replace with your actual DbContext

            public RadioStationService(DbContext context)
            {
                _context = context;
            }

        public async Task AddRadioStationAsync(RadioStationViewModel radioStation)
            {
                _context.Set<RadioStationViewModel>().Add(radioStation);
                await _context.SaveChangesAsync();
            }

            public async Task UpdateRadioStationAsync(RadioStationViewModel radioStation)
            {
                _context.Set<RadioStationViewModel>().Update(radioStation);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteRadioStationAsync(RadioStationViewModel radioStation)
            {
                _context.Set<RadioStationViewModel>().Remove(radioStation);
                await _context.SaveChangesAsync();
            }
        public async Task<RadioStationViewModel> GetRadioStationByIdAsync(int id)
        {
            return await _context.Set<RadioStationViewModel>()
                .Where(r => r.Id == id)
                .Select(r => new RadioStationViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    MailZip = r.MailZip,
                    PhyCity = r.PhyCity
                })
                .FirstOrDefaultAsync();
        }
    }
    }


